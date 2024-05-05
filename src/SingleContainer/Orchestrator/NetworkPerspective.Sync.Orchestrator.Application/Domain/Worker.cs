using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class Worker
{
    public Guid Id { get; }

    public DateTime CreatedAt { get; }

    public Worker(Guid id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }
}