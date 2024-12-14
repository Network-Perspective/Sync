using System;

namespace NetworkPerspective.Sync.Utils.CQS.Queries;

public interface IQuery<out TResponse>
    where TResponse : class
{
    string UserFriendlyName { get; set; }
    Guid CorrelationId { get; set; }
}
