using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Common.Tests.Factories;

public class TestableAzureKeyVaultClient
{
    public static IVault Create()
    {
        var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
        var secretRepository = new AzureKeyVaultClient(TokenCredentialFactory.Create(), secretRepositoryOptions, NullLogger<AzureKeyVaultClient>.Instance);
        return secretRepository;
    }
}