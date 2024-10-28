namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Configs;

public class JiraAuthConfig
{
    public string BaseUrl { get; set; }
    public string Path { get; set; }
    public string[] Scopes { get; set; } = [];
}