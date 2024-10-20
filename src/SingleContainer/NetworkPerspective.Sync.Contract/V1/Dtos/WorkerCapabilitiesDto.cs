using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class WorkerCapabilitiesDto : IResponse
{
    public Guid CorrelationId { get; set; }
    public IEnumerable<string> SupportedConnectorTypes { get; set; }
}