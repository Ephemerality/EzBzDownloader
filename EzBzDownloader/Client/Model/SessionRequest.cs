using Newtonsoft.Json;

namespace EzBzDownloader.Client.Model
{
    public sealed class SessionRequest
    {
        public SessionRequest(string email)
        {
            Identity = new Identity(email);
        }

        [JsonProperty("identity")]
        public Identity Identity { get; }

        [JsonProperty("clientInfo")]
        public ClientInfo ClientInfo { get; } = new();
    }

    public sealed class Identity
    {
        public Identity(string email)
        {
            Email = email;
        }

        [JsonProperty("identityType")]
        public string IdentityType { get; } = "accountEmail";

        [JsonProperty("email")]
        public string Email { get; }
    }

    public sealed class ClientInfo
    {
        [JsonProperty("deviceName")]
        public string DeviceName { get; } = "bzclient";

        [JsonProperty("clientType")]
        public string ClientType { get; } = "com/backblaze/backup/win";
    }
}