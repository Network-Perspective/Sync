using Microsoft.AspNetCore.Authentication;

namespace NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;

public class ApiKeyAuthOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public const string DefaultApiKeyVaultKey = "orchestrator-api-key";

    public string ApiKeyVaultKey { get; set; } = DefaultApiKeyVaultKey;
}