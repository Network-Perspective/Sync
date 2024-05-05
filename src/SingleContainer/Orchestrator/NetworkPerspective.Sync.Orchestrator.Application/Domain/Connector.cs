using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class Connector
{
    public Guid Id { get; }
    public Worker Worker { get; }
    public Guid NetworkId { get; }
    public DateTime CreatedAt { get; }

    protected Connector(Guid id, Worker worker, Guid networkId, DateTime createdAt)
    {
        Id = id;
        Worker = worker;
        NetworkId = networkId;
        CreatedAt = createdAt;
    }
}

public class Connector<T> : Connector
    where T : ConnectorProperties
{

    public T Properties { get; }

    private Connector(Guid id, Worker worker, Guid networkId, T properties, DateTime createdAt)
        : base(id, worker, networkId, createdAt)
    {
        Properties = properties;
    }

    public static Connector<TProperties> Create<TProperties>(Guid id, Worker worker, Guid networkId, TProperties properties, DateTime createdAt)
        where TProperties : ConnectorProperties
        => new Connector<TProperties>(id, worker, networkId, properties, createdAt);
}