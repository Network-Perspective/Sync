using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

public interface IRemoteConnectorClient
{
    /// <summary>
    /// Sends a message to a connector and does not wait for a reply
    /// </summary>
    /// <param name="connectorName"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task NotifyConnectorAsync(string connectorName, IMessage message);

    /// <summary>
    /// Calls a connector and waits for a reply
    /// </summary>
    /// <param name="connectorName"></param>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> CallConnectorAsync<T>(string connectorName, IRpcArgs message);
}