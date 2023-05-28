using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class ConversationJoinResponse : IResponseWithError
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("channel")]
        public JoinedConversation Conversation { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }

        internal class JoinedConversation
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}