using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault.Tests.Fixtures;

public class ExternalAzureKeyVaultFixture
{
    public IVault Client { get; }

    public ExternalAzureKeyVaultFixture()
    {
        var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
        var internalLogger = NullLogger<AzureKeyVaultClient>.Instance;
        var internalCredentials = TokenCredentialFactory.Create();
        var internalClient = new AzureKeyVaultClient(internalCredentials, secretRepositoryOptions, internalLogger);

        var externalOptions = Options.Create(new ExternalAzureKeyVaultConfig { BaseUrl = TestsConsts.ExternalAzureKeyVaultBaseUrl });
        var externalLogger = NullLogger<ExternalAzureKeyVaultClient>.Instance;
        Client = new ExternalAzureKeyVaultClient(internalClient, externalOptions, externalLogger);
    }
}