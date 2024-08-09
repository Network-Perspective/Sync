using Azure.Core;
using Azure.Identity;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;

public static class TokenCredentialFactory
{
    public static TokenCredential Create()
    {
        var azureCredentialsOptions = new DefaultAzureCredentialOptions
        {
            ExcludeAzureCliCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeInteractiveBrowserCredential = true,
            ExcludeEnvironmentCredential = true,
            ExcludeSharedTokenCacheCredential = true,
            ExcludeManagedIdentityCredential = false,
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = false,
        };

        return new DefaultAzureCredential(azureCredentialsOptions);
    }
}