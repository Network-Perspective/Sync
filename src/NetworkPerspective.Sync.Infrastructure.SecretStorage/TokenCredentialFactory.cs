using Azure.Core;
using Azure.Identity;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{
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
}