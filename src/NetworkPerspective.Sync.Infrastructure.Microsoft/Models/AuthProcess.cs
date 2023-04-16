using System;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    public class AuthProcess
    {
        public Guid NetworkId { get; }
        public Uri CallbackUri { get; }

        public AuthProcess(Guid networkId, Uri callbackUri)
        {
            NetworkId = networkId;
            CallbackUri = callbackUri;
        }
    }
}