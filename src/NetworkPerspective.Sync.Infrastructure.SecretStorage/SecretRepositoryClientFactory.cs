using System;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;

using HealthChecks.AzureKeyVault;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{
    interface ISecretRepositoryHealthCheckFactory
    {
        IHealthCheck CreateHealthCheck();
    }
    
    internal class SecretRepositoryClientFactory : ISecretRepositoryFactory, ISecretRepositoryHealthCheckFactory
    {
        private readonly TokenCredential _tokenCredential;
        private readonly ILoggerFactory _loggerFactory;
        private readonly INetworkService _networkService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<AzureKeyVaultConfig> _azureKvOptions;
        private readonly IOptions<HcpVaultConfig> _hcpVaultOptions;

        public SecretRepositoryClientFactory(TokenCredential tokenCredential,
            ILoggerFactory loggerFactory,
            INetworkService networkService,
            IServiceProvider serviceProvider,
            IOptions<AzureKeyVaultConfig> azureKvOptions,
            IOptions<HcpVaultConfig> hcpVaultOptions)
        {
            _tokenCredential = tokenCredential;
            _loggerFactory = loggerFactory;
            _networkService = networkService;
            _serviceProvider = serviceProvider;
            _azureKvOptions = azureKvOptions;
            _hcpVaultOptions = hcpVaultOptions;
        }

        public async Task<ISecretRepository> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            if (!string.IsNullOrEmpty(_azureKvOptions.Value.BaseUrl))
            {
                // else return internal or external secret storage based on network configuration
                var network = await _networkService.GetAsync<NetworkProperties>(networkId, stoppingToken);
                var externalKeyVaultUri = network.Properties.ExternalKeyVaultUri;

                if (externalKeyVaultUri is null)
                    return CreateInternalAzureKeyVaultClient();
                else
                    return CreateExternalAzureKeyVaultClient(externalKeyVaultUri);
            }
            else if (!string.IsNullOrEmpty(_hcpVaultOptions.Value.BaseUrl))
            {
                // create Hcp vault client
                return CreateHcpVaultClient();
            }

            return CreateDbSecretRepository();
        }
        
        public IHealthCheck CreateHealthCheck()
        {
            if (!string.IsNullOrEmpty(_azureKvOptions.Value.BaseUrl))
            {
                var options = new AzureKeyVaultOptions();
                options.AddSecret(_azureKvOptions.Value.TestSecretName);
                return new AzureKeyVaultHealthCheck(new Uri(_azureKvOptions.Value.BaseUrl), _tokenCredential, options);
            }

            if (!string.IsNullOrEmpty(_hcpVaultOptions.Value.BaseUrl))
            {
                return _serviceProvider.GetService<HcpVaultHealthCheck>();
            }

            return _serviceProvider.GetService<DbSecretRepositoryHealthCheck>();
        }

        private ISecretRepository CreateHcpVaultClient()
        {
            return _serviceProvider.GetService<HcpVaultClient>();
        }

        private ISecretRepository CreateDbSecretRepository()
        {
            return _serviceProvider.GetService<DbSecretRepositoryClient>();
        }

        private ISecretRepository CreateExternalAzureKeyVaultClient(Uri externalKeyVaultUri)
        {
            var internalKeyVault = CreateInternalAzureKeyVaultClient();
            var logger = _loggerFactory.CreateLogger<ExternalAzureKeyVaultClient>();

            return new ExternalAzureKeyVaultClient(externalKeyVaultUri, internalKeyVault, logger);
        }

        private ISecretRepository CreateInternalAzureKeyVaultClient()
        {
            var logger = _loggerFactory.CreateLogger<InternalAzureKeyVaultClient>();

            return new InternalAzureKeyVaultClient(_tokenCredential, _azureKvOptions, logger);
        }
    }
}