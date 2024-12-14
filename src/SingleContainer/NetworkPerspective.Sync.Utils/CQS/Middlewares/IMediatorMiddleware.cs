using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS.Middlewares;

public interface IMediatorMiddleware
{
    Task<TResponse> HandleQueryAsync<TRequest, TResponse>(TRequest request, QueryHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse;
}
