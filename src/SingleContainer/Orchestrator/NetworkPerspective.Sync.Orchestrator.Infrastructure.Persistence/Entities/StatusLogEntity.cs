using System;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

public class StatusLogEntity
{
    public long Id { get; set; }
    public Guid DataSourceId { get; set; }
    public DataSourceEntity DataSource { get; set; }
    public DateTime TimeStamp { get; set; }
    public string Message { get; set; }
    public int Level { get; set; }
}