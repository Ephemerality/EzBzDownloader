using System;
using OtpNet;

namespace EzBzDownloader.Lib
{
    public sealed class TotpGenerator
    {
        private readonly Totp _totp;

        public TotpGenerator(string secretKey)
        {
            _totp = new Totp(Base32Encoding.ToBytes(secretKey));
        }

        public string GenerateTotp() => _totp.ComputeTotp(DateTime.UtcNow);
    }
}