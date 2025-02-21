namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft;

internal static class MicrosoftKeys
{
    public const string MicrosoftTenantIdPattern = "microsoft-tenant-id-{0}";
    public const string MicrosoftClientBasicIdKey = "microsoft-client-basic-id";
    public const string MicrosoftClientBasicSecretKey = "microsoft-client-basic-secret";
    public const string MicrosoftClientTeamsIdKey = "microsoft-client-with-teams-id";
    public const string MicrosoftClientTeamsSecretKey = "microsoft-client-with-teams-secret";
    public const string UserTokenCacheKeyPattern = "microsoft-user-token-{0}";
    public const string UserKeyPattern = "microsoft-user-{0}";
}