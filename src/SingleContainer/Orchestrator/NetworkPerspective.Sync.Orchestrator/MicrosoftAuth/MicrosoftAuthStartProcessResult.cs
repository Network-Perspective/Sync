namespace NetworkPerspective.Sync.Orchestrator.MicrosoftAuth;

public class MicrosoftAuthStartProcessResult
{
    public string MicrosoftAuthUri { get; }

    public MicrosoftAuthStartProcessResult(string microsoftAuthUri)
    {
        MicrosoftAuthUri = microsoftAuthUri;
    }
}