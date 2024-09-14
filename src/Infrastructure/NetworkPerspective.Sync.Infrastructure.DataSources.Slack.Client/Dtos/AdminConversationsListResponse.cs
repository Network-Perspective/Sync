using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos
{
    public class AdminConversationsListResponse : IResponseWithError, ICursorPagination
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("conversations")]
        public IReadOnlyCollection<SingleConversation> Conversations { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }

        public class SingleConversation
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("is_private")]
            public bool IsPrivate { get; set; }

            [JsonProperty("member_count")]
            public int MembersCount { get; set; }
        }
    }
}