using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class NoOpMiddleware : IMediatorMiddleware
{
    Task IMediatorMiddleware.HandleAsync<TRequest>(TRequest request, Func<TRequest, CancellationToken, Task> next, CancellationToken cancellationToken)
        => next(request, cancellationToken);

    Task<TResponse> IMediatorMiddleware.HandleAsync<TRequest, TResponse>(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
        => next(request, cancellationToken);
}
