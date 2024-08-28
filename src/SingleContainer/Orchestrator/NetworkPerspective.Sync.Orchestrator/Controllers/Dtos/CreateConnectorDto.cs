using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Dtos
{
    public class CreateConnectorDto
    {
        public Guid Id { get; set; }
        public Guid NetworkId { get; set; }
        public Guid WorkerId { get; set; }
        public string Type { get; set; }
        public string AccessToken { get; set; }
        public IEnumerable<ConnectorPropertyDto> Properties { get; set; }
    }
}