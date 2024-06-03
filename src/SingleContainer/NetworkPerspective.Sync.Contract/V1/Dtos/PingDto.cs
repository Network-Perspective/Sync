using System;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class PingDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
}