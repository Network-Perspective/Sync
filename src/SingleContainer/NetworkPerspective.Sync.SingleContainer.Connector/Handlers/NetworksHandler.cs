using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.SingleContainer.Connector.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Connector.Handlers;

public class NetworksHandler(IHostConnection hostConnection, ILogger<NetworksHandler> logger) : IMessageHandler<AddNetwork>
{
    public async Task HandleAsync(AddNetwork msg)
    {
        logger.LogInformation("Adding network {networkId}", msg.NetworkId);

        await hostConnection.InvokeAsync(new Ping("Reply from connector"));
    }
}