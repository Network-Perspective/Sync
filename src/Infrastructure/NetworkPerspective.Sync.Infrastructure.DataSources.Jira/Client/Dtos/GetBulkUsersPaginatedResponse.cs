using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Pagination;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;

public class GetBulkUsersPaginatedResponse : IPaginatedResponse<GetBulkUsersPaginatedResponse.SingleUser>
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
    public IReadOnlyCollection<SingleUser> Values { get; set; }

    public class SingleUser
    {
        [JsonProperty("accountId")]
        public string Id { get; set; }

        [JsonProperty("emailAddress")]
        public string Email { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }
    }
}