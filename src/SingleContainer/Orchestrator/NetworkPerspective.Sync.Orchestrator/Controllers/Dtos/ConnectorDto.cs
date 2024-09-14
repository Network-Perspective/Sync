using System;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class ConnectorDto
{
    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public string Type { get; set; }

}