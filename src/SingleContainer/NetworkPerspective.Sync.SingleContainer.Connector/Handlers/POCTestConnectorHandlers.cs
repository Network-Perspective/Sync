using System.Text.Json;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.SingleContainer.Connector.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Connector.Handlers;

public class NetworksHandler(IHostConnection hostConnection, ILogger<NetworksHandler> logger, 
    IStatusLogRepository statusLogRepository) : IMessageHandler<AddNetwork>,
    IRpcHandler<IsAuthenticated, IsAuthenticatedResult>
{
    public async Task HandleAsync(AddNetwork msg)
    {
        logger.LogInformation("Adding network {networkId}", msg.NetworkId);

        await statusLogRepository.AddAsync(StatusLog.Create(msg.NetworkId, "Network added", StatusLogLevel.Info,
            DateTime.UtcNow));
        
        await hostConnection.NotifyAsync(new Ping("Ping from connector"));

        var result = await hostConnection.CallAsync<FindNetworkResult>(new FindNetwork(Guid.NewGuid()));
        logger.LogInformation("Found network {networkId}", JsonSerializer.Serialize(result));
    }

    public Task<IsAuthenticatedResult> HandleAsync(IsAuthenticated args)
    {
        return Task.FromResult(new IsAuthenticatedResult("Yes we are! (" + args.Message + ")"));
    }
}