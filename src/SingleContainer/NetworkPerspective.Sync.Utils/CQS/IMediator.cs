using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS;

public interface IMediator
{
    Task SendCommandAsync<TCommand>(TCommand request, CancellationToken stoppingToken = default)
        where TCommand : class, ICommand;

    Task<TResponse> SendQueryAsync<TQuery, TResponse>(TQuery request, CancellationToken stoppingToken = default)
        where TQuery : class, IQuery<TResponse>
        where TResponse : class, IResponse;
}
