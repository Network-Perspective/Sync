using NetworkPerspective.Sync.SingleContainer.Host.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Host.Handlers;

public class PingHandler(ILogger<PingHandler> logger, IConnectorContext context) : IMessageHandler<Ping>
{
    public Task HandleAsync(Ping msg)
    {
        logger.LogInformation("Received ping message: {message} from {connectorName}",
            msg.Message, context.Name);
        return Task.CompletedTask;
    }
}