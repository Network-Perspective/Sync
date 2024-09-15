namespace NetworkPerspective.Sync.Orchestrator.OAuth.Microsoft;

public class MicrosoftAuthStartProcessResult
{
    public string MicrosoftAuthUri { get; }

    public MicrosoftAuthStartProcessResult(string microsoftAuthUri)
    {
        MicrosoftAuthUri = microsoftAuthUri;
    }
}