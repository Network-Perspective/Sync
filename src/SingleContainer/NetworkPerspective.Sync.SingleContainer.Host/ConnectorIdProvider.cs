using Microsoft.AspNetCore.SignalR;

namespace NetworkPerspective.Sync.Orchestrator
{
    public class ConnectorIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User.FindFirst("connectorId")?.Value;
        }
    }
}
