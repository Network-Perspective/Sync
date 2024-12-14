using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Worker.Application.Domain.OAuth;

public class OAuthContext
{
    public ConnectorContext Connector { get; }
    public string CallbackUri { get; }

    public OAuthContext(ConnectorContext connector, string callbackUri)
    {
        Connector = connector;
        CallbackUri = callbackUri;
    }
}