namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

public interface IRemoteConnectorClient
{
    Task InvokeConnectorAsync(string connectorName, IMessage message);
}