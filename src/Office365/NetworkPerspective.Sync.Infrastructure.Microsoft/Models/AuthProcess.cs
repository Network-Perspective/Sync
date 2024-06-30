using System;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    public class AuthProcess
    {
        public Guid ConnectorId { get; }
        public Uri CallbackUri { get; }

        public AuthProcess(Guid connectorId, Uri callbackUri)
        {
            ConnectorId = connectorId;
            CallbackUri = callbackUri;
        }
    }
}