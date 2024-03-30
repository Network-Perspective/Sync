using System.Collections.Concurrent;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Connector.Transport;

public interface IHostConnection
{
    /// <summary>
    /// Send a message to the host and don't wait for a reply
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task NotifyAsync(IMessage message);

    /// <summary>
    /// Call a method on the host and wait for a reply
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> CallAsync<T>(IRpcArgs message);
}

public interface IHostConnectionInternal : IHostConnection
{
    Task ConnectorReply(string correlationId, IRpcResult message);
    Task HandleHostReply(string name, string correlationId, string payload);
}

public class HostConnection(HubConnection hubConnection, IMessageSerializer messageSerializer,
    ILogger<HostConnection> logger) : IHostConnectionInternal
{
    public async Task NotifyAsync(IMessage message)
    {
        var (name, payload) = messageSerializer.Serialize(message);
        logger.LogDebug("Sending " + name + " with " + payload + " to host");
        await hubConnection.SendAsync("NotifyHost", name, payload);
    }

    private readonly ConcurrentDictionary<string, TaskCompletionSource<IRpcResult>> _runningRpcCalls = new();
    public async Task<T> CallAsync<T>(IRpcArgs message)
    {
        var (name, payload) = messageSerializer.Serialize(message);
        logger.LogDebug("Sending " + name + " with " + payload + " to host");

        var correlationId = Guid.NewGuid().ToString();
        await hubConnection.SendAsync("CallHost", correlationId, name, payload, typeof(T).Name);

        // wait for reply
        TaskCompletionSource<IRpcResult> tcs = new();
        _runningRpcCalls.TryAdd(correlationId, tcs);
        return (T)await tcs.Task;
    }

    public async Task ConnectorReply(string correlationId, IRpcResult message)
    {
        var (name, payload) = messageSerializer.Serialize(message);
        logger.LogDebug("Replying " + name + " with " + payload + " to host with correlationId " + correlationId);
        await hubConnection.SendAsync("ConnectorReply", correlationId, name, payload);
    }

    public async Task HandleHostReply(string correlationId, string name, string payload)
    {
        logger.LogDebug("Received reply for " + correlationId + " with " + payload);
        var message = messageSerializer.Deserialize(name, payload) as IRpcResult;
        if (_runningRpcCalls.Remove(correlationId, out TaskCompletionSource<IRpcResult> methodCallCompletionSource))
        {
            methodCallCompletionSource.SetResult(message);
        }
    }

}