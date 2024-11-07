using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class ConnectorDto
{
    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public string Type { get; set; }
}

public class ConnectorDetailsDto : ConnectorDto
{
    public IDictionary<string, string> Properties { get; set; }
}