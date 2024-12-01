using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS.Middlewares;

public interface IMediatorMiddleware
{
    Task HandleCommandAsync<TRequest>(TRequest request, CommandHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
        where TRequest : class, ICommand;

    Task<TResponse> HandleQueryAsync<TRequest, TResponse>(TRequest request, QueryHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        where TRequest : class, IQuery<TResponse>
        where TResponse : class, IResponse;
}
