using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class GetConnectorStatusDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Guid ConnectorId { get; set; }
    public IDictionary<string, string> ConnectorProperties { get; set; }
}