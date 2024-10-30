namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Configs;

public class JiraConfig
{
    public string BaseUrl { get; set; }
    public JiraAuthConfig Auth { get; set; } = new();
}