using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class NoOpMiddleware : IMediatorMiddleware
{
    Task IMediatorMiddleware.HandleCommandAsync<TRequest>(TRequest request, CommandHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
        => next(request, cancellationToken);

    Task<TResponse> IMediatorMiddleware.HandleQueryAsync<TRequest, TResponse>(TRequest request, QueryHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        => next(request, cancellationToken);
}
