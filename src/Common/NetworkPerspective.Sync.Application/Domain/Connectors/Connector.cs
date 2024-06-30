using System;

namespace NetworkPerspective.Sync.Application.Domain.Connectors
{
    public class Connector
    {
        public Guid Id { get; }
        public DateTime CreatedAt { get; }

        protected Connector(Guid id, DateTime createdAt)
        {
            Id = id;
            CreatedAt = createdAt;
        }
    }

    public class Connector<T> : Connector where T : ConnectorProperties
    {

        public T Properties { get; }

        private Connector(Guid id, T properties, DateTime createdAt) : base(id, createdAt)
        {
            Properties = properties;
        }

        public static Connector<TProperties> Create<TProperties>(Guid connectorId, TProperties properties, DateTime createdAt) where TProperties : ConnectorProperties
            => new Connector<TProperties>(connectorId, properties, createdAt);
    }
}