namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

public interface IRemoteConnectorClientInternal : IRemoteConnectorClient
{
    Task HandleConnectorReply(string correlationId, string name, string payload);
    Task HostReplyAsync(string connectionId, string correlationId, IMessage message);
}