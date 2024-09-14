using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination
{
    public class MetadataResponse
    {
        [JsonProperty("next_cursor")]
        public string NextCursor { get; set; }
    }
}