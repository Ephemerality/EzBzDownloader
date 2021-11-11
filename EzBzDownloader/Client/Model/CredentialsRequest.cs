using Newtonsoft.Json;

namespace EzBzDownloader.Client.Model
{
    public sealed class CredentialsRequest
    {
        [JsonProperty("credentials")]
        public CredentialInfo Credentials { get; set; }

        [JsonProperty("infoRequested")]
        public string[] InfoRequested { get; set; }

        [JsonProperty("authToken")]
        public string AuthToken { get; set; }

        public abstract class CredentialInfo
        {
            [JsonProperty("credentialsType")]
            public abstract CredentialType Type { get; }

            public sealed class Password : CredentialInfo
            {
                public override CredentialType Type => CredentialType.password;

                public Password(string password)
                {
                    Value = password;
                }

                [JsonProperty("password")]
                public string Value { get; set; }
            }

            public sealed class Totp : CredentialInfo
            {
                public override CredentialType Type => CredentialType.totp;

                public Totp(string code)
                {
                    Value = code;
                }

                [JsonProperty("code")]
                public string Value { get; }
            }
        }
    }
}