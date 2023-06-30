﻿using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class ReactionsGetResponse : IResponseWithError
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("message")]
        public MessageObject Message { get; set; }

        internal class MessageObject
        {
            [JsonProperty("reactions")]
            public IReadOnlyCollection<Reaction> Reactions { get; set; } = Array.Empty<Reaction>();

            [JsonProperty("created")]
            public string Created { get; set; }

            [JsonProperty("timestamp")]
            public string Timestamp { get; set; }
        }

        internal class Reaction
        {
            [JsonProperty("users")]
            public IReadOnlyCollection<string> Users { get; set; }
        }
    }
}