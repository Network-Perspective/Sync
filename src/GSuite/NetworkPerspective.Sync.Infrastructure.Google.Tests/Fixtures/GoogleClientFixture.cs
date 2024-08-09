using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Google.Services;
using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Fixtures
{
    public class GoogleClientFixture
    {
        public ICredentialsProvider CredentialProvider { get; }

        public GoogleClientFixture()
        {
            var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
            var secretRepository = new AzureKeyVaultClient(TokenCredentialFactory.Create(), secretRepositoryOptions, NullLogger<AzureKeyVaultClient>.Instance);
            CredentialProvider = new CredentialsProvider(secretRepository);
        }
    }
}