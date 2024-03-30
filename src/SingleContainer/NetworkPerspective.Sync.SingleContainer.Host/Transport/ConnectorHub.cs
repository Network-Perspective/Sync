using Microsoft.AspNetCore.SignalR;

using NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Host.Transport;

public interface IConnectorClient
{
    Task InvokeConnector(string method, string payload);
}

public class ConnectorHub(ILogger<ConnectorHub> logger, IServiceProvider services,
    IRemoteConnectorClient connectorClient) : Hub<IConnectorClient>
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
    
    public async Task CallHost(string correlationId, string method, string payload, string returnType)
    {
        logger.LogDebug(Context.ConnectionId + " called " + method + " with " + payload);

        // initialize scope and connector context
        using IServiceScope scope = services.CreateScope();
        var connectorContextProvider = scope.ServiceProvider.GetRequiredService<IConnectorContextProvider>();
        connectorContextProvider.Initialize(Context.ConnectionId);

        // dispatch message
        var dispatcher = scope.ServiceProvider.GetRequiredService<IRpcDispatcher>();
        var result = await dispatcher.CallRpc(method, payload, returnType);
        await connectorClient.HostReplyAsync(Context.ConnectionId, correlationId, result);
        
        // var handler = scope.ServiceProvider.GetRequiredService<IFindNetworkRpc>();
        //
        // var message = new MessageSerializer().Deserialize(method, payload);
        // var result = await handler.Handle(message as FindNetwork);
        //
        // await connectorClient.HostReplyAsync(Context.ConnectionId, correlationId, result);
    }
}