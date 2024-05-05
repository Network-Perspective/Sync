using System;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class StartSyncDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}