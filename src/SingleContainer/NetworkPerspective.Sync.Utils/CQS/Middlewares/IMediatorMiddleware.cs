using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS.Middlewares;

public interface IMediatorMiddleware
{
    Task HandleCommandAsync<TCommand>(TCommand command, CommandHandlerDelegate<TCommand> next, CancellationToken cancellationToken)
        where TCommand : class, ICommand;

    Task<TResponse> HandleQueryAsync<TQuery, TResponse>(TQuery query, QueryHandlerDelegate<TQuery, TResponse> next, CancellationToken cancellationToken)
        where TQuery : class, IQuery<TResponse>
        where TResponse : class, IResponse;
}
