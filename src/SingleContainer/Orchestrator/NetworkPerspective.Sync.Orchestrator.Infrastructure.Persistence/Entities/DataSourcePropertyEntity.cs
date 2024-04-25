using System;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

public class DataSourcePropertyEntity
{
    public long Id { get; set; }
    public Guid DataSourceId { get; set; }
    public DataSourceEntity DataSource { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}