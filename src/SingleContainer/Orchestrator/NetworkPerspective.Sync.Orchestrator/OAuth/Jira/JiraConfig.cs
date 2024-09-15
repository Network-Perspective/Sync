namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira;

public class JiraConfig
{
    public string BaseUrl { get; set; }
    public JiraAuthConfig Auth { get; set; } = new();
}

public class JiraAuthConfig
{
    public string Path { get; set; }
    public string[] Scopes { get; set; } = [];
}