using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Application.Domain.SecretRotation;

public class SecretRotationContext
{
    private readonly IDictionary<string, string> _connectorProperties;

    public Guid ConnectorId { get; }

    public SecretRotationContext(Guid connectorId, IDictionary<string, string> connectorProperties)
    {
        ConnectorId = connectorId;
        _connectorProperties = connectorProperties;
    }

    public T GetConnectorProperties<T>() where T : ConnectorProperties, new()
        => ConnectorProperties.Create<T>(_connectorProperties);

    public ConnectorProperties GetConnectorProperties()
        => ConnectorProperties.Create<ConnectorProperties>(_connectorProperties);
}