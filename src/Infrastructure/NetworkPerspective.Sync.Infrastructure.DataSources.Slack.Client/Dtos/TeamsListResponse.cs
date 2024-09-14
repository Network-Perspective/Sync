using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos
{
    public class TeamsListResponse : IResponseWithError, ICursorPagination
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("teams")]
        public IReadOnlyCollection<SingleTeam> Teams { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }

        public class SingleTeam
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}