using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EzBzDownloader.Client.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CredentialType
    {
        password,
        totp
    }
}