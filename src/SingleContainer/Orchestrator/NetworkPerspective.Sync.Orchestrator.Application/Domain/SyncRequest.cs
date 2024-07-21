using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class SyncRequest
{
    public Guid ConnectorId { get; set; }
    public List<Employee> Employees { get; set; }
}