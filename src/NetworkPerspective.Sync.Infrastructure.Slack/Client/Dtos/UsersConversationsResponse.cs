using System.Collections.Generic;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class UsersConversationsResponse : IResponseWithError, ICursorPagination
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("channels")]
        public IReadOnlyCollection<SingleConversation> Conversations { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }

        internal class SingleConversation
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("is_private")]
            public bool IsPrivate { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}