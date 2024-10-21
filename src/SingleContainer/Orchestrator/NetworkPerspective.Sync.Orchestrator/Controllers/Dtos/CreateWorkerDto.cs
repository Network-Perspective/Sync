using System;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class CreateWorkerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Secret { get; set; }
}