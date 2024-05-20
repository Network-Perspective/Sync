using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Exceptions;

public class ConnectorNotFoundException : Exception
{
    public Guid ConnectorId { get; }

    public ConnectorNotFoundException(Guid connectorId)
        : base($"Connector '{connectorId}' not found")
    {
        ConnectorId = connectorId;
    }
}
