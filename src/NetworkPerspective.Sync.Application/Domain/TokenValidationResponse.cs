using System;

namespace NetworkPerspective.Sync.Application.Domain
{
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
}