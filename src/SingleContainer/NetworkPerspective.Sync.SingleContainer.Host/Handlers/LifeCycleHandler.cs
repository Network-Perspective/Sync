using NetworkPerspective.Sync.SingleContainer.Host.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Host.Handlers;

public class LifeCycleHandler(ILogger<LifeCycleHandler> logger,
    IConnectorPool pool,
    IConnectorContextProvider contextProvider) :
    IMessageHandler<RegisterConnector>
{
    public Task HandleAsync(RegisterConnector msg)
    {
        logger.LogInformation("Registering connector " + msg.Name);
        pool.RegisterConnector(contextProvider.ConnectionId, msg);
        return Task.CompletedTask;
    }


}