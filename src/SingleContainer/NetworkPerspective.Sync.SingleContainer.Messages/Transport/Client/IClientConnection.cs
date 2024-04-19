using System;
using System.Threading.Tasks;

using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Commands;
using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Queries;

namespace NetworkPerspective.Sync.SingleContainer.Messages.Transport.Client;

public interface IClientConnection
{
    Task SendAsync<TCommand>(Guid clientId, TCommand command)
        where TCommand : ICommand;

    Task<TQueryResult> QueryAsync<TQueryArgs, TQueryResult>(Guid clientId, IQueryArgs message)
        where TQueryArgs : IQueryArgs
        where TQueryResult : IQueryResult;
}