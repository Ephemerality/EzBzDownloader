using System;
using System.Security.Cryptography;
using System.Text;

namespace EzBzDownloader.Lib
{
    public static class ByteExtensions
    {
        public static string ToHex(this byte[] buffer, bool lower = true)
        {
            var result = new StringBuilder(buffer.Length * 2);
            var format = lower ? "{0:x2}" : "{0:X2}";
            foreach (var b in buffer)
                result.AppendFormat(format, b);
            return result.ToString();
        }

        public static string ToHex(this byte[] buffer, int index, int count, bool lower = true)
            => buffer[index..count].ToHex(lower);

        public static byte[] ComputeSha1(this byte[] buffer)
        {
            using var sha1 = HashAlgorithm.Create(nameof(SHA1));
            if (sha1 == null)
                throw new Exception("Failed to initialize SHA1");

            return sha1.ComputeHash(buffer, 0, buffer.Length);
        }
    }
}