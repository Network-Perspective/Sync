using System;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Entities;

public class SecretEntity
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }

}