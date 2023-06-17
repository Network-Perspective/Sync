using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class ConversationRepliesResponse : IResponseWithError, ICursorPagination
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("has_more")]
        public bool HasMoreMessages { get; set; }

        [JsonProperty("messages")]
        public IReadOnlyCollection<SingleMessage> Messages { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }

        internal class SingleMessage
        {
            public string MessageId => HashCode.Combine(ThreadTimestamp, TimeStamp, User).ToString();

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("thread_ts")]
            public string ThreadTimestamp { get; set; }

            [JsonProperty("ts")]
            public string TimeStamp { get; set; }

            [JsonProperty("reactions")]
            public IReadOnlyCollection<ReactionToMessage> Reactions { get; set; } = Array.Empty<ReactionToMessage>();

            internal class ReactionToMessage
            {
                [JsonProperty("users")]
                public IReadOnlyCollection<string> Users { get; set; } = Array.Empty<string>();
            }
        }
    }
}