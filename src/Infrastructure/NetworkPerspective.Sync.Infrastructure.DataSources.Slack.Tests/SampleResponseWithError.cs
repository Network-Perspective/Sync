using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Tests
{
    public class SampleResponseWithError : IResponseWithError
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}