using System;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class GetWorkerCapabilitiesDto : IRequest
{
    public Guid CorrelationId { get; set; }
}