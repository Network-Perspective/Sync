using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Common.Tests;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault.Tests.Fixtures;

public class AzureKeyVaultFixture
{
    public AzureKeyVaultClient Client { get; }

    public AzureKeyVaultFixture()
    {
        var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
        var internalLogger = NullLogger<AzureKeyVaultClient>.Instance;
        var internalCredentials = TokenCredentialFactory.Create();
        Client = new AzureKeyVaultClient(internalCredentials, secretRepositoryOptions, internalLogger);
    }
}