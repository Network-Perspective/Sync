using System.Collections.Generic;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class ConversationMembersResponse : IResponseWithError, ICursorPagination
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("members")]
        public IReadOnlyCollection<string> Members { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }
    }
}