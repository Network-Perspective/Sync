using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class MetadataResponse
    {
        [JsonProperty("next_cursor")]
        public string NextCursor { get; set; }
    }
}