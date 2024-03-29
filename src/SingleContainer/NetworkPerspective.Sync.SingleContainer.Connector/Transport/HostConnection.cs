using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Connector.Transport;

public interface IHostConnection
{
    Task InvokeAsync(IMessage message);
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
}