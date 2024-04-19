using System.Threading.Tasks;

using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Commands;
using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Queries;

namespace NetworkPerspective.Sync.SingleContainer.Messages.Transport.Server;

public interface IHubConnection
{
    Task NotifyAsync<TCommand>(TCommand command)
        where TCommand : ICommand;

    Task<TQueryResult> CallAsync<TQueryArgs, TQueryResult>(IQueryArgs query)
        where TQueryArgs : IQueryArgs
        where TQueryResult : IQueryResult;
}