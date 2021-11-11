using EzBzDownloader.Client.Logic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace EzBzDownloader.Client.Model
{
    public sealed class SessionResponse
    {
        [JsonProperty("apiUrl")]
        public string ApiUrl { get; set; }

        [JsonConverter(typeof(BzDateConverter))]
        [JsonProperty("asOf")]
        public string AsOf { get; set; }

        [JsonProperty("authToken")]
        public string AuthToken { get; set; }

        [JsonProperty("challenge")]
        public Challenge Challenge { get; set; }

        [JsonProperty("clientToken")]
        [CanBeNull]
        public string ClientToken { get; set; }

        [JsonProperty("credentialProblem")]
        [CanBeNull]
        public CredentialProblem CredentialProblem { get; set; }

        [JsonConverter(typeof(BzDateConverter))]
        [JsonProperty("idleTimeout")]
        public string IdleTimeout { get; set; }

        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("isAuthenticated")]
        public bool IsAuthenticated { get; set; }

        [JsonProperty("scope")]
        public Scope[] Scope { get; set; }

        [JsonConverter(typeof(BzDateConverter))]
        [JsonProperty("sessionExpires")]
        public string SessionExpires { get; set; }
    }

    public sealed class AccountProfile
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("accountStanding")]
        public string AccountStanding { get; set; }

        [JsonProperty("authSetting")]
        public string AuthSetting { get; set; }

        [JsonProperty("canUserReferFriends")]
        public bool CanUserReferFriends { get; set; }

        [JsonProperty("clusterNum")]
        public string ClusterNum { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("emailReceipts")]
        public bool EmailReceipts { get; set; }

        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("enabledServices")]
        public string[] EnabledServices { get; set; }

        [JsonProperty("groupsApiEnabledAsOf")]
        [CanBeNull]
        public object GroupsApiEnabledAsOf { get; set; }

        [JsonProperty("infoType")]
        public string InfoType { get; set; }

        [JsonProperty("isBackblazeCertifiedReseller")]
        public bool IsBackblazeCertifiedReseller { get; set; }

        [JsonProperty("isInB2Suspend")]
        public bool IsInB2Suspend { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("memberOfGroupId")]
        [CanBeNull]
        public object MemberOfGroupId { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("phoneVerified")]
        public bool PhoneVerified { get; set; }
    }

    public sealed class CredentialProblem
    {
        [JsonProperty("credentialProblemType")]
        public CredentialType Type { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    public sealed class Info
    {
        [JsonProperty("accountProfile")]
        [CanBeNull]
        public AccountProfile AccountProfile { get; set; }
    }

    public sealed class Scope
    {
        [JsonProperty("scopeType")]
        public string ScopeType { get; set; }
    }

    public sealed class Challenge
    {
        [JsonProperty("challengeType")]
        public CredentialType ChallengeType { get; set; }

        [JsonProperty("smsFallbackAllowed")]
        public bool? SmsFallbackAllowed { get; set; }
    }
}