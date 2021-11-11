using System.Text;
using EzBzDownloader.Lib;

namespace EzBzDownloader.Client.Logic
{
    public static class BzSanity
    {
        public static string Calculate(string email)
        {
            var result = new StringBuilder(4);
            var sha1 = Encoding.UTF8.GetBytes(Encoding.UTF8.GetBytes(email).ToHex()).ComputeSha1().ToHex();
            result.Append(sha1[1]);
            result.Append(sha1[3]);
            result.Append(sha1[5]);
            result.Append(sha1[7]);
            return result.ToString();
        }
    }
}