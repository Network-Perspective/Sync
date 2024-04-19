using System.Threading.Tasks;

using NetworkPerspective.Sync.SingleContainer.Messages.CQS;
using NetworkPerspective.Sync.SingleContainer.Messages.Transport.Client;

namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

public interface IRemoteConnectorClientInternal : IClientConnection
{
    Task HandleConnectorReply(string correlationId, string name, string payload);
    Task HostReplyAsync(string connectionId, string correlationId, IMessage message);
}