using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class OAuthInitializationResult
{
    public string AuthUri { get; }
    public string State { get; }
    public DateTime StateExpirationTimestamp { get; }

    public OAuthInitializationResult(string authUri, string state, DateTime stateExpirationTimestamp)
    {
        AuthUri = authUri;
        State = state;
        StateExpirationTimestamp = stateExpirationTimestamp;
    }
}