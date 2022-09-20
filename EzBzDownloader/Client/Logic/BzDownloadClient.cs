using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EzBzDownloader.Client.Model;
using EzBzDownloader.Lib;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace EzBzDownloader.Client.Logic
{
    public sealed class BzDownloadClient
    {
        private readonly HttpClient _client;

        private const string BaseUrl = "https://api.backblazeb2.com/b2api/v1";
        private const string CreateSessionUrl = "/b2_create_session";
        private const string PresentCredentialsUrl = "/b2api/v1/b2_present_credentials";

        private const string RestoreInfoUrl = "https://ca001.backblaze.com/api/restoreinfo";

        private const string Version = "8.0.1.555";

        private SessionResponse _session;

        private readonly string _username;
        private readonly string _password;
        [CanBeNull]
        private readonly string _totpKey;
        [CanBeNull]
        private readonly StringPrompt _stringPrompt;

        public BzDownloadClient(string username, string password, [CanBeNull] string totpKey, [CanBeNull] StringPrompt stringPrompt)
        {
            _username = username;
            _password = password;
            _totpKey = totpKey;
            _stringPrompt = stringPrompt;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _client.DefaultRequestHeaders.UserAgent.ParseAdd($"backblaze_agent/{Version}");
        }

        public async Task<Restore[]> ListRestoresAsync(string email, CancellationToken cancellationToken)
        {
            if (IsSessionInvalid())
                await LoginAsync(cancellationToken);

            var message = new HttpRequestMessage(HttpMethod.Post, RestoreInfoUrl);
            message.Headers.Add("Expect", "100-continue");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Version), "version");
            content.Add(new StringContent(Encoding.UTF8.GetBytes(email).ToHex()), "hexemailaddr");
            content.Add(new StringContent("none"), "hexpassword");
            content.Add(new StringContent("none"), "twofactorverifycode");
            content.Add(new StringContent("none"), "hexsecondfactor");
            content.Add(new StringContent("none"), "bz_auth_token");
            content.Add(new StringContent(_session.AuthToken), "bz_v5_auth_token");
            content.Add(new StringContent(BzSanity.Calculate(email)), "bzsanity");
            content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("dummy")), "bzpostp", "bzdatap.zip");
            message.Content = content;

            var response = await SendAsync(message, cancellationToken);

            return XmlUtil.Deserialize<Content>(await response.Content.ReadAsStreamAsync(cancellationToken)).Restores;
        }

        public async Task DownloadRestoreAsync(Restore restore, string destinationPath, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting download of {restore.DisplayFilename} ({restore.Zipsize / 1024 / 1024:N}MB)");
            var filename = Path.Combine(destinationPath, restore.DisplayFilename);
            Console.WriteLine($"Writing to {filename}");
            var validateExisting = File.Exists(filename);

            await using var file = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var fileLock = new SemaphoreSlim(1);

            await using var metadata = File.Open($"{filename}.metadata", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var metadataLock = new SemaphoreSlim(1);

            var chunkCount = (int) Math.Ceiling(restore.Zipsize / (double) BzChunk.ChunkSize);

            // Load list of chunks that are already downloaded
            Dictionary<int, BzChunkMetadata> chunkMetadata = null;

            var completed = 0;

            if (validateExisting && metadata.Length > 0)
            {
                chunkMetadata = await BzChunkMetadataHandler.LoadMetadataAsync(metadata, cancellationToken);
                Console.WriteLine($"Loaded metadata for {chunkMetadata.Count} chunks of {chunkCount}, validating file...");
                await BzChunkMetadataHandler.ValidateChunksAsync(file, chunkMetadata, cancellationToken);
                Console.WriteLine($"{chunkMetadata.Count}/{chunkCount} valid chunks already downloaded");
                completed += chunkMetadata.Count;
            }

            await Enumerable.Range(0, chunkCount)
                // Only download chunks that aren't in the metadata list
                .Where(i => chunkMetadata == null || !chunkMetadata.ContainsKey(i))
                .ParallelForEachAsync(async i =>
                {
                    var startPosition = i * (long) BzChunk.ChunkSize;
                    var size = Math.Min(restore.Zipsize - startPosition, BzChunk.ChunkSize);
                    var chunk = await DownloadChunkAsync(restore, startPosition, size, cancellationToken);

                    try
                    {
                        await fileLock.WaitAsync(cancellationToken);
                        if (file.Position != startPosition)
                            file.Seek(startPosition, SeekOrigin.Begin);
                        await file.WriteAsync(chunk.Content.AsMemory(0, chunk.Content.Length), cancellationToken);
                        await file.FlushAsync(cancellationToken);
                    }
                    finally
                    {
                        fileLock.Release();
                    }

                    try
                    {
                        await metadataLock.WaitAsync(cancellationToken);
                        metadata.Seek(i * chunk.Metadata.Sha1.Length, SeekOrigin.Begin);
                        await metadata.WriteAsync(chunk.Metadata.Sha1.AsMemory(0, chunk.Metadata.Sha1.Length), cancellationToken);
                        await metadata.FlushAsync(cancellationToken);
                    }
                    finally
                    {
                        metadataLock.Release();
                    }

                    Interlocked.Increment(ref completed);
                    Console.WriteLine($"{completed}/{chunkCount}");
                }, 0, cancellationToken);
        }

        private async Task<BzChunk> DownloadChunkAsync(Restore restore, long index, long size, CancellationToken cancellationToken)
        {
            if (IsSessionInvalid())
                await LoginAsync(cancellationToken);

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{restore.Serverhost}.backblaze.com/api/restorezipdownloadex");
            request.Headers.Add("Expect", "100-continue");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Version), "version");
            content.Add(new StringContent(Encoding.UTF8.GetBytes(_session.Info.AccountProfile!.Email).ToHex()), "hexemailaddr");
            content.Add(new StringContent(Encoding.UTF8.GetBytes("NULL").ToHex()), "hexpassword");
            content.Add(new StringContent(_session.AuthToken), "bz_v5_auth_token");
            content.Add(new StringContent(BzSanity.Calculate(_session.Info.AccountProfile.Email)), "bzsanity");
            content.Add(new StringContent(restore.Hguid), "hguid");
            content.Add(new StringContent(restore.Rid), "rid");
            content.Add(new StringContent(index.ToString()), "request_firstbyteindex");
            content.Add(new StringContent(size.ToString()), "request_numbytes");
            content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("dummy")), "bzpostp", "bzdatap.zip");
            request.Content = content;

            var response = await SendAsync(request, cancellationToken);
            return BzFtpDecoder.Decode(await response.Content.ReadAsStreamAsync(cancellationToken));
        }

        private async Task LoginAsync(CancellationToken cancellationToken)
        {
            await LoginInternalAsync(cancellationToken);
            if (_session == null)
                throw new Exception("No session after login");
            if (_session.Info.AccountProfile == null)
                throw new Exception("Session missing account profile");
        }

        private async Task LoginInternalAsync(CancellationToken cancellationToken)
        {
            // Clear out old session
            _session = null;

            var sessionRequest = new SessionRequest(_username);
            var message = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}{CreateSessionUrl}");
            message.Content = new StringContent(JsonConvert.SerializeObject(sessionRequest), Encoding.UTF8);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await _client.SendAsync(message, cancellationToken);
            var sessionResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _session = JsonConvert.DeserializeObject<SessionResponse>(sessionResponse)
                       ?? throw new Exception($"Failed to deserialize session response: {sessionResponse}");

            var credentialsRequest = new CredentialsRequest
            {
                Credentials = new CredentialsRequest.CredentialInfo.Password(_password),
                InfoRequested = new[] { "accountProfile" },
                AuthToken = _session.AuthToken
            };

            message = new HttpRequestMessage(HttpMethod.Post, $"{_session.ApiUrl}{PresentCredentialsUrl}");
            message.Content = new StringContent(JsonConvert.SerializeObject(credentialsRequest), Encoding.UTF8);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            response = await _client.SendAsync(message, cancellationToken);
            sessionResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _session = JsonConvert.DeserializeObject<SessionResponse>(sessionResponse)
                       ?? throw new Exception($"Failed to deserialize session response: {sessionResponse}");

            if (_session.Challenge != null)
            {
                if (_session.Challenge.ChallengeType != CredentialType.totp)
                    throw new Exception("?");
            }

            if (_session.IsAuthenticated)
                return;

            string totp = null;
            if (!string.IsNullOrWhiteSpace(_totpKey))
            {
                var totpProvider = new TotpGenerator(_totpKey);
                totp = totpProvider.GenerateTotp();
            }
            else if (_stringPrompt != null)
                totp = _stringPrompt.Invoke("Enter TOTP code:");

            if (string.IsNullOrWhiteSpace(totp))
                throw new Exception("2FA was required but no TOTP key or code was entered");

            credentialsRequest = new CredentialsRequest
            {
                Credentials = new CredentialsRequest.CredentialInfo.Totp(totp),
                InfoRequested = new[] { "accountProfile" },
                AuthToken = _session.AuthToken
            };

            message = new HttpRequestMessage(HttpMethod.Post, $"{_session.ApiUrl}{PresentCredentialsUrl}");
            message.Content = new StringContent(JsonConvert.SerializeObject(credentialsRequest), Encoding.UTF8);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            response = await _client.SendAsync(message, cancellationToken);
            sessionResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _session = JsonConvert.DeserializeObject<SessionResponse>(sessionResponse)
                       ?? throw new Exception($"Failed to deserialize session response: {sessionResponse}");
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            if (IsSessionInvalid())
                await LoginAsync(cancellationToken);

            message.Headers.TryAddWithoutValidation("Authorization", _session.AuthToken);

            return await _client.SendAsync(message, cancellationToken);
        }

        // TOTO check expiration
        private bool IsSessionInvalid() => _session == null;
    }
}