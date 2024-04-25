using System;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

public class SyncHistoryEntryEntity
{
    public long Id { get; set; }
    public Guid DataSourceId { get; set; }
    public DataSourceEntity DataSource { get; set; }
    public DateTime TimeStamp { get; set; }
    public DateTime SyncPeriodStart { get; set; }
    public DateTime SyncPeriodEnd { get; set; }
    public double? SuccessRate { get; set; }
    public long InteractionsCount { get; set; }
    public int TasksCount { get; set; }
}