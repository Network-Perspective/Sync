using System;

namespace NetworkPerspective.Sync.Utils.CQS.Queries;

public interface IRequest<out TResponse>
    where TResponse : class
{
    string UserFriendlyName { get; set; }
    Guid CorrelationId { get; set; }
}