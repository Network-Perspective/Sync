namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

public interface IRemoteConnectorClient
{
    Task InvokeConnectorAsync(string connectorName, IMessage message);
    Task<T> CallConnectorAsync<T>(string connectorName, IRpcArgs message);
    Task HandleConnectorReply(string correlationId, string name, string payload);
    Task HostReplyAsync(string connectionId, string correlationId, IMessage message);
}