using System;

namespace NetworkPerspective.Sync.Worker.Application.Domain;

public class CoreTokenValidationResult
{
    public Guid ConnectorId { get; }
    public Guid NetworkId { get; }

    public CoreTokenValidationResult(Guid connectorId, Guid networkId)
    {
        ConnectorId = connectorId;
        NetworkId = networkId;
    }
}