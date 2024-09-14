using Microsoft.AspNetCore.SignalR;

namespace NetworkPerspective.Sync.Orchestrator.Extensions
{
    public static class HubCallerContextExtensions
    {
        public static string GetWorkerName(this HubCallerContext context)
        {
            return context.User.Identity.Name;
        }
    }
}