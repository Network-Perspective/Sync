using System;

namespace NetworkPerspective.Sync.Worker.Application.Domain.OAuth;

public class InitializeOAuthResult
{
    public string AuthUri { get; set; }
    public string State { get; }
    public DateTime StateExpirationTimestamp { get; }

    public InitializeOAuthResult(string authUri, string state, DateTime stateExpirationTimestamp)
    {
        AuthUri = authUri;
        State = state;
        StateExpirationTimestamp = stateExpirationTimestamp;
    }
}