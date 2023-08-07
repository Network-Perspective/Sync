using System;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{
    internal class AzureKeyVaultClientFactory : ISecretRepositoryFactory
    {
        private readonly TokenCredential _tokenCredential;
        private readonly ILoggerFactory _loggerFactory;
        private readonly INetworkService _networkService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<AzureKeyVaultConfig> _options;

        public AzureKeyVaultClientFactory(TokenCredential tokenCredential,
            ILoggerFactory loggerFactory,
            INetworkService networkService,
            IServiceProvider serviceProvider,
            IOptions<AzureKeyVaultConfig> options)
        {
            _tokenCredential = tokenCredential;
            _loggerFactory = loggerFactory;
            _networkService = networkService;
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public async Task<ISecretRepository> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            // if keyvault is not configured fallback to db storage
            if (string.IsNullOrEmpty(_options.Value.BaseUrl))
            {
                return CreateDbSecretRepository();
            }

            // else return internal or external secret storage based on network configuration
            var network = await _networkService.GetAsync<NetworkProperties>(networkId, stoppingToken);
            var externalKeyVaultUri = network.Properties.ExternalKeyVaultUri;

            if (externalKeyVaultUri is null)
                return CreateInternal();
            else
                return CreateExternal(externalKeyVaultUri);
        }

        private ISecretRepository CreateDbSecretRepository()
        {
            return _serviceProvider.GetService<DbSecretRepositoryClient>();
        }

        private ISecretRepository CreateExternal(Uri externalKeyVaultUri)
        {
            var internalKeyVault = CreateInternal();
            var logger = _loggerFactory.CreateLogger<ExternalAzureKeyVaultClient>();

            return new ExternalAzureKeyVaultClient(externalKeyVaultUri, internalKeyVault, logger);
        }

        private ISecretRepository CreateInternal()
        {
            var logger = _loggerFactory.CreateLogger<InternalAzureKeyVaultClient>();

            return new InternalAzureKeyVaultClient(_tokenCredential, _options, logger);
        }
    }
}