using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Worker.Application.Domain.OAuth;

public class OAuthContext
{
    public ConnectorInfo Connector { get; }
    public string CallbackUri { get; }

    public OAuthContext(ConnectorInfo connector, string callbackUri)
    {
        Connector = connector;
        CallbackUri = callbackUri;
    }
}