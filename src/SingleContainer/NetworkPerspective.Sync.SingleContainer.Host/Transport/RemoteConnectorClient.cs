using Microsoft.AspNetCore.SignalR;

using NetworkPerspective.Sync.SingleContainer.Host.Controllers;
using NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Host.Transport;

public class RemoteConnectorClient(IHubContext<ConnectorHub> hubContext, IMessageSerializer messageSerializer,
    IConnectorPool connectorPool)
    : IRemoteConnectorClient
{
    public async Task InvokeConnectorAsync(string connectorName, IMessage message)
    {
        var (name, payload) = messageSerializer.Serialize(message);
        var connectionId = connectorPool.FindConnectionId(connectorName);

        await hubContext
            .Clients
            .Clients(connectionId)
            .SendAsync("InvokeConnector", name, payload);
    }

}