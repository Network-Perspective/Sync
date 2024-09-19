namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira;

public class JiraAuthStartProcessResult
{
    public string JiraAuthUri { get; }

    public JiraAuthStartProcessResult(string jiraAuthUri)
    {
        JiraAuthUri = jiraAuthUri;
    }
}