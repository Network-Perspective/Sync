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

    private readonly Dictionary<string, TaskCompletionSource<IRpcResult>> _runningRpcCalls = new();
    public async Task<T> CallConnectorAsync<T>(string connectorName, IRpcArgs message)
    {
        var (name, payload) = messageSerializer.Serialize(message);
        var connectionId = connectorPool.FindConnectionId(connectorName);

        var correlationId = Guid.NewGuid().ToString();
        await hubContext
            .Clients
            .Clients(connectionId)
            .SendAsync("CallConnector", correlationId, name, payload, typeof(T).Name);

        // wait for reply
        TaskCompletionSource<IRpcResult> tcs = new();
        _runningRpcCalls.TryAdd(correlationId, tcs);
        return (T)await tcs.Task;
    }

    public async Task HandleConnectorReply(string correlationId, string name, string payload)
    {
        var message = messageSerializer.Deserialize(name, payload) as IRpcResult;
        if (_runningRpcCalls.Remove(correlationId, out TaskCompletionSource<IRpcResult> methodCallCompletionSource))
        {
            methodCallCompletionSource.SetResult(message);
        }
        else
        {
            throw new InvalidOperationException("No running rpc call for " + correlationId);
        }
    }

    public async Task HostReplyAsync(string connectionId, string correlationId, IMessage message)
    {
        var (name, payload) = messageSerializer.Serialize(message);

        await hubContext
            .Clients
            .Clients(connectionId)
            .SendAsync("HostReply", correlationId, name, payload);
    }

}