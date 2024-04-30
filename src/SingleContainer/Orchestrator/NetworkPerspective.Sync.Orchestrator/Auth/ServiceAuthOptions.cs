using Microsoft.AspNetCore.Authentication;

namespace NetworkPerspective.Sync.Orchestrator.Auth;

public class ServiceAuthOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ServiceAuthScheme";
}