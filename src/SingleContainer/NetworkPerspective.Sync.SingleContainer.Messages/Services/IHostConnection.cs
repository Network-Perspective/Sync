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