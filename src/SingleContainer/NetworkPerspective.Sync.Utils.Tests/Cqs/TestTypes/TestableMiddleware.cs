using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class TestableMiddleware : IMediatorMiddleware
{
    public static int CalledCount = 0;

    Task<TResponse> IMediatorMiddleware.HandleQueryAsync<TRequest, TResponse>(TRequest request, QueryHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        CalledCount++;
        return next(request, cancellationToken);
    }

    public static void Reset()
        => CalledCount = 0;
}
