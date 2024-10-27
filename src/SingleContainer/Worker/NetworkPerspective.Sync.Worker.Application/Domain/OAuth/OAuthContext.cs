using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Worker.Application.Domain.OAuth;

public class OAuthContext
{
    private readonly IEnumerable<KeyValuePair<string, string>> _connectorProperties;

    public Guid ConnectorId { get; }
    public string CallbackUri { get; }

    public OAuthContext(Guid connectorId, string callbackUri, IEnumerable<KeyValuePair<string, string>> connectorProperties)
    {
        ConnectorId = connectorId;
        CallbackUri = callbackUri;
        _connectorProperties = connectorProperties;
    }

    public T GetConnectorProperties<T>() where T : ConnectorProperties, new()
        => ConnectorProperties.Create<T>(_connectorProperties);
}
