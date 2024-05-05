using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

public class ConnectorEntity
{
    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public WorkerEntity Worker { get; set; }
    public Guid NetworkId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ConnectorPropertyEntity> Properties { get; set; }
    public ICollection<SyncHistoryEntryEntity> SyncHistory { get; set; }
}