using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos
{
    public class OAuthExchangeResponse : IResponseWithError
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}