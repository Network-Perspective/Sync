﻿using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos
{
    public class ConversationMembersResponse : IResponseWithError, ICursorPagination
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