using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class PingDto : IQuery<PongDto>
{
    public string UserFriendlyName { get; set; } = "Ping";
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
}