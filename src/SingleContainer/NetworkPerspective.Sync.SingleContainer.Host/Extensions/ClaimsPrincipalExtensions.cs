using System;
using Microsoft.AspNetCore.SignalR;

namespace NetworkPerspective.Sync.Orchestrator.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetConnectorId(this HubCallerContext context)
        {
            var claim = context.User.FindFirst(x => x.Type == "ConnectorId");

            return new Guid(claim.Value);
        }
    }
}
