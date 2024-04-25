using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class DataSource
{
    public Guid Id { get; }
    public Guid ConnectorId { get; }
    public Guid NetworkId { get; }
    public DateTime CreatedAt { get; }

    protected DataSource(Guid id, Guid connectorId, Guid networkId, DateTime createdAt)
    {
        Id = id;
        ConnectorId = connectorId;
        NetworkId = networkId;
        CreatedAt = createdAt;
    }
}

public class DataSource<T> : DataSource
    where T : DataSourceProperties
{

    public T Properties { get; }

    private DataSource(Guid id, Guid connectorId, Guid networkId, T properties, DateTime createdAt)
        : base(id, connectorId, networkId, createdAt)
    {
        Properties = properties;
    }

    public static DataSource<TProperties> Create<TProperties>(Guid id, Guid connectorId, Guid networkId, TProperties properties, DateTime createdAt) where TProperties : DataSourceProperties
        => new DataSource<TProperties>(id, connectorId, networkId, properties, createdAt);
}