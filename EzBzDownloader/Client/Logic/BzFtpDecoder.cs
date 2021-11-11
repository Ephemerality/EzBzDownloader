using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using EzBzDownloader.Client.Model;
using EzBzDownloader.Lib;

namespace EzBzDownloader.Client.Logic
{
    public static class BzFtpDecoder
    {
        private const string HeaderValue = "bzftp001t_aaaaaabzftp002";
        private const string FooterValue = "bzftpend";
        private const string ShaMarker = "bzftpsha";
        private const int ShaLength = 40;
        private static readonly int FooterLength = FooterValue.Length + ShaMarker.Length + ShaLength;
        private static readonly int MinimumStreamLength = HeaderValue.Length + FooterValue.Length + ShaMarker.Length + ShaLength;

        private static readonly Regex ShaRegex = new($"{ShaMarker}(?<sha1>[0-9a-f]{{40}}){FooterValue}", RegexOptions.Compiled);

        public static BzChunk Decode(Stream bzFtpEncodedStream)
        {
            if (!bzFtpEncodedStream.CanSeek || bzFtpEncodedStream.Position != 0)
                throw new ArgumentException("Stream must be seekable or be at the beginning", nameof(bzFtpEncodedStream));

            if (bzFtpEncodedStream.Length <= MinimumStreamLength)
                throw new Exception($"Stream too short (only {bzFtpEncodedStream.Length} bytes vs. the expected {MinimumStreamLength} for header/footer and at least 1 byte of data)");

            if (bzFtpEncodedStream.CanSeek && bzFtpEncodedStream.Position != 0)
                bzFtpEncodedStream.Seek(0, SeekOrigin.Begin);

            var headerFooterBuffer = new byte[FooterLength];
            bzFtpEncodedStream.Read(headerFooterBuffer, 0, HeaderValue.Length);
            if (Encoding.UTF8.GetString(headerFooterBuffer, 0, HeaderValue.Length) != HeaderValue)
                throw new Exception($"Invalid header: {headerFooterBuffer.ToHex(0, HeaderValue.Length)}");

            var dataLength = bzFtpEncodedStream.Length - MinimumStreamLength;
            var buffer = new byte[dataLength];
            bzFtpEncodedStream.Read(buffer);

            bzFtpEncodedStream.Read(headerFooterBuffer, 0, FooterLength);
            var match = ShaRegex.Match(Encoding.UTF8.GetString(headerFooterBuffer, 0, FooterLength));
            if (!match.Success)
                throw new Exception($"Invalid footer: {headerFooterBuffer.ToHex(0, FooterLength)}");

            var computedSha1 = buffer.ComputeSha1();

            if (match.Groups["sha1"].Value != computedSha1.ToHex())
                throw new Exception($"Invalid sha1 hash: expected {computedSha1.ToHex()}, found {match.Groups["sha1"].Value}");

            return new BzChunk(buffer, new BzChunkMetadata(computedSha1));
        }
    }
}