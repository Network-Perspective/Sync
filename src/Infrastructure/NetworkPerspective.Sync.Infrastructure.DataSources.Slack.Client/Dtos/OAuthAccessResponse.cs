using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos
{
    public class OAuthAccessResponse : IResponseWithError
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("authed_user")]
        public AuthedUser User { get; set; }

        public class AuthedUser
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }
    }
}