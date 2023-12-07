using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Models
{
    public class AuthProcess
    {
        public Guid NetworkId { get; }
        public Uri CallbackUri { get; }
        public bool RequireAdminPrivileges { get; }

        public AuthProcess(Guid networkId, Uri callbackUri, bool requireAdminPrivileges)
        {
            NetworkId = networkId;
            CallbackUri = callbackUri;
            RequireAdminPrivileges = requireAdminPrivileges;
        }
    }
}