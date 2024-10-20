using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class WorkerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsAuthorized { get; set; }
    public bool IsOnline { get; set; }
    public IEnumerable<string> SupportedConnectorTypes { get; set; }
}