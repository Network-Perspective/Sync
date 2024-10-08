﻿using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos
{
    public class ConversationHistoryResponse : IResponseWithError, ICursorPagination
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("messages")]
        public IReadOnlyCollection<SingleMessage> Messages { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }

        public class SingleMessage
        {
            public string MessageId => HashCode.Combine(TimeStamp, User).ToString();

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("subtype")]
            public string Subtype { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("ts")]
            public string TimeStamp { get; set; }

            [JsonProperty("latest_reply")]
            public string LatestReplyTimeStamp { get; set; }

            [JsonProperty("reply_count")]
            public int ReplyCount { get; set; }

            [JsonProperty("room")]
            public Room VoiceChat { get; set; }

            [JsonProperty("reactions")]
            public IReadOnlyCollection<ReactionToMessage> Reactions { get; set; } = Array.Empty<ReactionToMessage>();

            public class ReactionToMessage
            {
                [JsonProperty("users")]
                public IReadOnlyCollection<string> Users { get; set; } = Array.Empty<string>();
            }

            public class Room
            {
                [JsonProperty("date_start")]
                public long Start { get; set; }

                [JsonProperty("date_end")]
                public long End { get; set; }

                [JsonProperty("participant_history")]
                public IReadOnlyCollection<string> Participants { get; set; } = Array.Empty<string>();
            }
        }
    }
}