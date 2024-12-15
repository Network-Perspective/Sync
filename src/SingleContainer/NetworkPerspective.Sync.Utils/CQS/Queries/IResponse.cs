using System;

namespace NetworkPerspective.Sync.Utils.CQS.Queries;

public interface IResponse
{
    Guid CorrelationId { get; set; }
}