using NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Handlers;

public class PingHandler(ILogger<PingHandler> logger, IConnectorContext context) : IMessageHandler<Ping>
{
    public Task HandleAsync(Ping msg)
    {
        logger.LogInformation("Received ping message: {message} from {connectorName}",
            msg.Message, context.Name);
        return Task.CompletedTask;
    }
}