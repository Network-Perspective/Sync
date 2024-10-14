using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Pagination;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;

public class GetProjectsPaginatedResponse : IPaginatedResponse<GetProjectsPaginatedResponse.SingleProject>
{
    [JsonProperty("isLast")]
    public bool IsLast { get; set; }

    [JsonProperty("nextPage")]
    public string NextPage { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("startAt")]
    public int StartAt { get; set; }

    [JsonProperty("maxResults")]
    public int MaxResults { get; set; }

    [JsonProperty("values")]
    public IReadOnlyCollection<SingleProject> Values { get; set; }

    public class SingleProject
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}