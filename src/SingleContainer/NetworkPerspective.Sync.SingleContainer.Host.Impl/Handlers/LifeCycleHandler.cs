using NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Handlers;

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