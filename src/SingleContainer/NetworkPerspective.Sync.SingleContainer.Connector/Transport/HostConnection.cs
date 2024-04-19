using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.SingleContainer.Messages.CQS;
using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Queries;
using NetworkPerspective.Sync.SingleContainer.Messages.Transport.Server;

namespace NetworkPerspective.Sync.SingleContainer.Connector.Transport;

public interface IHostConnectionInternal : IHubConnection
{
    Task ConnectorReply(string correlationId, IQueryResult message);
    Task HandleHostReply(string name, string correlationId, string payload);
}

public class HostConnection(HubConnection hubConnection, IMessageSerializer messageSerializer,
    ILogger<HostConnection> logger) : IHostConnectionInternal
{
    public async Task NotifyAsync(IMessage message)
    {
        hubConnection.
        var (name, payload) = messageSerializer.Serialize(message);
        logger.LogDebug("Sending " + name + " with " + payload + " to host");
        await hubConnection.SendAsync("NotifyHost", name, payload);
    }

    public async Task<T> CallAsync<T>(IQueryArgs message)
    {
        return await hubConnection.InvokeAsync<T>("CallHost", message);
    }
    }

}