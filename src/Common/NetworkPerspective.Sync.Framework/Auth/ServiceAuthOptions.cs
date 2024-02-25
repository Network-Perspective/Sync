using Microsoft.AspNetCore.Authentication;

namespace NetworkPerspective.Sync.Framework.Auth
{
    public class ServiceAuthOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ServiceAuthScheme";
    }
}