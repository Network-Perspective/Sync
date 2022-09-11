﻿using System.Collections.Generic;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class UsersListResponse : IResponseWithError, ICursorPagination
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("members")]
        public IReadOnlyCollection<SingleUser> Members { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }// = MetadataResponse.Empty;

        internal class SingleUser
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("team_id")]
            public string TeamId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("profile")]
            public Profile Profile { get; set; }

            [JsonProperty("is_bot")]
            public bool IsBot { get; set; }

            [JsonProperty("is_invited_user")]
            public bool IsInvited { get; set; }
        }

        internal class Profile
        {
            [JsonProperty("email")]
            public string Email { get; set; }
        }
    }
}