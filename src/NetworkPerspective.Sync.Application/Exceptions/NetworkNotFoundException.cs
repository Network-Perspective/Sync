using System;

namespace NetworkPerspective.Sync.Application.Exceptions
{
    public class NetworkNotFoundException : ApplicationException
    {
        public Guid NetworkId { get; }

        public NetworkNotFoundException(Guid networkId) : base($"Network '{networkId}' not found")
        {
            NetworkId = networkId;
        }
    }
}