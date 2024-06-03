namespace NetworkPerspective.Sync.Orchestrator.SlackAuth;

public class SlackAuthStartProcessResult
{
    public string SlackAuthUri { get; }

    public SlackAuthStartProcessResult(string slackAuthUri)
    {
        SlackAuthUri = slackAuthUri;
    }
}