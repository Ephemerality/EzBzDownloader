using JetBrains.Annotations;

namespace EzBzDownloader.Client.Model
{
    public sealed class Config
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SecretKey { get; set; }
        [CanBeNull]
        public string DestinationPath { get; set; }
    }
}