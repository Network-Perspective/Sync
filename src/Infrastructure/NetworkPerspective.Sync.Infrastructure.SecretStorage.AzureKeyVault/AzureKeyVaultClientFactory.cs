using System;

using Azure.Core;

using HealthChecks.AzureKeyVault;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.AzureKeyVault;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage;

internal class AzureKeyVaultClientFactory : ISecretRepositoryFactory
{
    private readonly TokenCredential _tokenCredential;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptions<AzureKeyVaultConfig> _azureKvOptions;

    public AzureKeyVaultClientFactory(TokenCredential tokenCredential,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IOptions<AzureKeyVaultConfig> azureKvOptions)
    {
        _tokenCredential = tokenCredential;
        _loggerFactory = loggerFactory;
        _azureKvOptions = azureKvOptions;
    }

    public ISecretRepository Create(Uri externalKeyVaultUri = null)
    {
        return externalKeyVaultUri is null
            ? CreateInternalAzureKeyVaultClient()
            : CreateExternalAzureKeyVaultClient(externalKeyVaultUri);
    }

    public IHealthCheck CreateHealthCheck()
    {
        var options = new AzureKeyVaultOptions();
        options.AddSecret(_azureKvOptions.Value.TestSecretName);
        return new AzureKeyVaultHealthCheck(new Uri(_azureKvOptions.Value.BaseUrl), _tokenCredential, options);
    }

    private ExternalAzureKeyVaultClient CreateExternalAzureKeyVaultClient(Uri externalKeyVaultUri)
    {
        var internalKeyVault = CreateInternalAzureKeyVaultClient();
        var logger = _loggerFactory.CreateLogger<ExternalAzureKeyVaultClient>();

        return new ExternalAzureKeyVaultClient(externalKeyVaultUri, internalKeyVault, logger);
    }

    private InternalAzureKeyVaultClient CreateInternalAzureKeyVaultClient()
    {
        var logger = _loggerFactory.CreateLogger<InternalAzureKeyVaultClient>();

        return new InternalAzureKeyVaultClient(_tokenCredential, _azureKvOptions, logger);
    }
}