using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class Connector
{
    public Guid Id { get; }
    public string Type { get; }
    public IDictionary<string, string> Properties { get; }
    public Worker Worker { get; }
    public Guid NetworkId { get; }
    public DateTime CreatedAt { get; }

    public Connector(Guid id, string type, IDictionary<string, string> properties, Worker worker, Guid networkId, DateTime createdAt)
    {
        Id = id;
        Type = type;
        Properties = properties;
        Worker = worker;
        NetworkId = networkId;
        CreatedAt = createdAt;
    }
}