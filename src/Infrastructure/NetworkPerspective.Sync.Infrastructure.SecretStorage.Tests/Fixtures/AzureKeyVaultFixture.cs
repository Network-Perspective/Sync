using System;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.AzureKeyVault;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.AzureKeyVault.Tests.Fixtures;

public class AzureKeyVaultFixture
{
    public InternalAzureKeyVaultClient InternalClient { get; }
    public ExternalAzureKeyVaultClient ExternalClient { get; }

    public AzureKeyVaultFixture()
    {
        var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
        var internalLogger = NullLogger<InternalAzureKeyVaultClient>.Instance;
        var internalCredentials = TokenCredentialFactory.Create();
        InternalClient = new InternalAzureKeyVaultClient(internalCredentials, secretRepositoryOptions, internalLogger);

        var externalUri = new Uri(TestsConsts.ExternalAzureKeyVaultBaseUrl);
        var externalLogger = NullLogger<ExternalAzureKeyVaultClient>.Instance;
        ExternalClient = new ExternalAzureKeyVaultClient(externalUri, InternalClient, externalLogger);
    }
}