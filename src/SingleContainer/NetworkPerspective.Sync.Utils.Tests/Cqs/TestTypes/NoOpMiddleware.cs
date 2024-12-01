using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class NoOpMiddleware : IMediatorMiddleware
{
    Task IMediatorMiddleware.HandleAsync<TRequest>(TRequest request, CommandHandler<TRequest> next, CancellationToken cancellationToken)
        => next(request, cancellationToken);

    Task<TResponse> IMediatorMiddleware.HandleAsync<TRequest, TResponse>(TRequest request, QueryHandler<TRequest, TResponse> next, CancellationToken cancellationToken)
        => next(request, cancellationToken);
}
