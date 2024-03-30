using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Connector.Transport;

public interface IHostConnection
{
    Task InvokeAsync(IMessage message);
    Task<T> CallAsync<T>(IRpcArgs message);
    Task HandleHostReply(string name, string correlationId, string payload);
}

public class HostConnection(HubConnection hubConnection, IMessageSerializer messageSerializer,
    ILogger<HostConnection> logger) : IHostConnection
{
    public async Task InvokeAsync(IMessage message)
    {
        var (name, payload) = messageSerializer.Serialize(message);
        logger.LogDebug("Sending " + name + " with " + payload + " to host");
        await hubConnection.SendAsync("InvokeHost", name, payload);
    }

    private  Dictionary<string, TaskCompletionSource<IRpcResult>> _runningRpcCalls = new();
    public async Task<T> CallAsync<T>(IRpcArgs message)
    {
        var (name, payload) = messageSerializer.Serialize(message);
        logger.LogDebug("Sending " + name + " with " + payload + " to host");
        
        var correlationId = Guid.NewGuid().ToString();
        await hubConnection.SendAsync("CallHost",correlationId, name,  payload, typeof(T).Name);
        
        // wait for reply
        TaskCompletionSource<IRpcResult> tcs = new();
        _runningRpcCalls.TryAdd(correlationId, tcs);
        return (T) await tcs.Task ;
    }

    public async Task HandleHostReply( string correlationId, string name, string payload)
    {
        logger.LogDebug("Received reply for " + correlationId + " with " + payload);
        var message = messageSerializer.Deserialize(name, payload) as IRpcResult;
        if (_runningRpcCalls.Remove(correlationId, out TaskCompletionSource<IRpcResult> methodCallCompletionSource))
        {
            methodCallCompletionSource.SetResult(message);
        }
    }

}