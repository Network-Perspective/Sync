namespace NetworkPerspective.Sync.Orchestrator.OAuth.Slack;

public class SlackAuthStartProcessResult
{
    public string SlackAuthUri { get; }

    public SlackAuthStartProcessResult(string slackAuthUri)
    {
        SlackAuthUri = slackAuthUri;
    }
}