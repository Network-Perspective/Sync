using Microsoft.AspNetCore.Authentication;

namespace NetworkPerspective.Sync.Orchestrator.Auth.Worker;

public class WorkerAuthOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "Worker";
}