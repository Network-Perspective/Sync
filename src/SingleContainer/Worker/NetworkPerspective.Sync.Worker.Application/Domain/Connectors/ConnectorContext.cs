using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

public class ConnectorContext
{
    private readonly IDictionary<string, string> _properties;

    public Guid ConnectorId { get; }
    public string Type { get; }

    public ConnectorContext(Guid id, string type, IDictionary<string, string> properties)
    {
        ConnectorId = id;
        Type = type;
        _properties = properties;
    }

    public T GetConnectorProperties<T>() where T : ConnectorProperties, new()
        => ConnectorProperties.Create<T>(_properties);

    public ConnectorProperties GetConnectorProperties()
        => ConnectorProperties.Create<ConnectorProperties>(_properties);
}