using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

public class WorkerEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string SecretHash { get; set; }
    public string SecretSalt { get; set; }
    public bool IsAuthorized { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ConnectorEntity> Connectors { get; set; }
}