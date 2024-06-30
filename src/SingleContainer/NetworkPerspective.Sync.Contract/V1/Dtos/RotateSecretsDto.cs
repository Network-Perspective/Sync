using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class RotateSecretsDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Guid ConnectorId { get; set; }
    public IDictionary<string, string> NetworkProperties { get; set; }
    public string ConnectorType { get; set; }
}