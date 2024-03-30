using System.Text.Json;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.SingleContainer.Connector.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Connector.Handlers;

public class NetworksHandler(IHostConnection hostConnection, ILogger<NetworksHandler> logger) : IMessageHandler<AddNetwork>,
    IRpcHandler<IsAuthenticated, IsAuthenticatedResult>
{
    public async Task HandleAsync(AddNetwork msg)
    {
        logger.LogInformation("Adding network {networkId}", msg.NetworkId);

        await hostConnection.InvokeAsync(new Ping("Ping from connector"));

        var result = await hostConnection.CallAsync<FindNetworkResult>(new FindNetwork(Guid.Empty));
        logger.LogInformation("Found network {networkId}", JsonSerializer.Serialize(result));
    }

    public async Task<IsAuthenticatedResult> HandleAsync(IsAuthenticated args)
    {
        return new IsAuthenticatedResult("Yes we are! (" + args.Message + ")");
    }
}