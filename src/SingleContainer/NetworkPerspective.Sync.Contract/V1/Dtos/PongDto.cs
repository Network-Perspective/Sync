using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class PongDto : IResponse
{
    public Guid CorrelationId { get; set; }
    public DateTime PingTimestamp { get; set; }
}