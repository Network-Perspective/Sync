using System;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class WorkerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsAuthorized { get; set; }
}