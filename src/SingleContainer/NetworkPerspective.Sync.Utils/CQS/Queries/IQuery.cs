using System;

namespace NetworkPerspective.Sync.Utils.CQS.Queries;

public interface IQuery<out TResponse>
    where TResponse : class
{
    Guid CorrelationId { get; set; }
}
