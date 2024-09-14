using System;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors
{
    public class ConnectorInfo
    {
        public Guid Id { get; }
        public Guid NetworkId { get; }

        public ConnectorInfo(Guid id, Guid networkId)
        {
            Id = id;
            NetworkId = networkId;
        }

        public override string ToString()
        {
            return $"{Id}:{NetworkId}";
        }
    }
}