using System;

namespace NetworkPerspective.Sync.Application.Domain.Networks
{
    public class Network
    {
        public Guid NetworkId { get; }
        public DateTime CreatedAt { get; }

        protected Network(Guid networkId, DateTime createdAt)
        {
            NetworkId = networkId;
            CreatedAt = createdAt;
        }
    }

    public class Network<T> : Network where T : NetworkProperties
    {

        public T Properties { get; }

        private Network(Guid networkId, T properties, DateTime createdAt) : base(networkId, createdAt)
        {
            Properties = properties;
        }

        public static Network<TProperties> Create<TProperties>(Guid networkId, TProperties properties, DateTime createdAt) where TProperties : NetworkProperties
            => new Network<TProperties>(networkId, properties, createdAt);
    }
}