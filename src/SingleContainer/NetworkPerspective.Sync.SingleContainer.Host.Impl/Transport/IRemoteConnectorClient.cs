namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

public interface IRemoteConnectorClient
{
    Task InvokeConnectorAsync(string connectorName, IMessage message);
    Task HostReplyAsync(string connectionId, string correlationId, IMessage message);
}