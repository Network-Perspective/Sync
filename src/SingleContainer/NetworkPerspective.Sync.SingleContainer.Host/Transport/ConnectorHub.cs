using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Host.Transport;

using Microsoft.AspNetCore.SignalR;

public interface IConnectorClient
{
    Task InvokeConnector(string method, string payload);
}

public class ConnectorHub(ILogger<ConnectorHub> logger, IServiceProvider services) : Hub<IConnectorClient>
{
    public async Task InvokeHost(string method, string payload)
    {
        logger.LogDebug(Context.ConnectionId + " invoked " + method + " with " + payload);

        // initialize scope and connector context
        using IServiceScope scope = services.CreateScope();
        var connectorContextProvider = scope.ServiceProvider.GetRequiredService<IConnectorContextProvider>();
        connectorContextProvider.Initialize(Context.ConnectionId);

        // dispatch message
        var dispatcher = scope.ServiceProvider.GetRequiredService<IMessageDispatcher>();
        await dispatcher.DispatchMessage(method, payload);
    }
}


public interface IRemoteConnectorClient
{
    Task InvokeConnectorAsync(string connectorName, IMessage message);
}

public class RemoteRemoteConnectorClient(IHubContext<ConnectorHub> hubContext, IMessageSerializer messageSerializer,
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