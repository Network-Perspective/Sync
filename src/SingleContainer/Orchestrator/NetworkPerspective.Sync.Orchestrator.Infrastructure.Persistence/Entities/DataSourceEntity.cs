using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

public class DataSourceEntity
{
    public Guid Id { get; set; }
    public Guid ConnectorId { get; set; }
    public Guid NetworkId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<DataSourcePropertyEntity> Properties { get; set; }
    public ICollection<SyncHistoryEntryEntity> SyncHistory { get; set; }
}