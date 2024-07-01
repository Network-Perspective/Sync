using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Dtos
{
    public class CreateConnectorDto
    {
        public Guid WorkerId { get; set; }
        public string Type { get; set; }
        public IEnumerable<ConnectorPropertyDto> Properties { get; set; }
    }
}