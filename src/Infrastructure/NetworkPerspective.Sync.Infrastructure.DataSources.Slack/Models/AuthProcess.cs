using System;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Models
{
    public class AuthProcess
    {
        public Guid ConnectorId { get; }
        public Uri CallbackUri { get; }
        public bool RequireAdminPrivileges { get; }

        public AuthProcess(Guid connectorId, Uri callbackUri, bool requireAdminPrivileges)
        {
            ConnectorId = connectorId;
            CallbackUri = callbackUri;
            RequireAdminPrivileges = requireAdminPrivileges;
        }
    }
}