using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;

public class JiraUser
{
    [JsonProperty("accountId")]
    public string Id { get; set; }

    [JsonProperty("active")]
    public bool IsActive { get; set; }
}