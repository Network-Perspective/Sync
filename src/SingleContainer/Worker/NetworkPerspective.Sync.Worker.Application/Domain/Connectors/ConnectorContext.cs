using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

public class ConnectorContext
{
    public IDictionary<string, string> Properties { get; }

    public Guid ConnectorId { get; }
    public string Type { get; }

    public ConnectorContext(Guid id, string type, IDictionary<string, string> properties)
    {
        ConnectorId = id;
        Type = type;
        Properties = properties;
    }
}