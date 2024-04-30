using System;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract;

public class TokenValidationResponse
{
    public Guid NetworkId { get; }
    public Guid ConnectorId { get; }

    public TokenValidationResponse(Guid networkId, Guid connectorId)
    {
        NetworkId = networkId;
        ConnectorId = connectorId;
    }
}
