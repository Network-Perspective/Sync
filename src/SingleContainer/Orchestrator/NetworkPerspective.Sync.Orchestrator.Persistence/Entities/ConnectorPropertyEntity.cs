using System;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Entities;

public class ConnectorPropertyEntity
{
    public long Id { get; set; }
    public Guid ConnectorId { get; set; }
    public ConnectorEntity Connector { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}