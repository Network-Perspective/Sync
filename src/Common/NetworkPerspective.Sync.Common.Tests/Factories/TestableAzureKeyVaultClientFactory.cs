using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Common.Tests.Factories
{
    public class TestableAzureKeyVaultClientFactory : ISecretRepositoryFactory
    {
        public Task<ISecretRepository> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
            var secretRepository = new InternalAzureKeyVaultClient(TokenCredentialFactory.Create(), secretRepositoryOptions, NullLogger<InternalAzureKeyVaultClient>.Instance);
            return Task.FromResult(secretRepository as ISecretRepository);
        }

        public ISecretRepository CreateDefault()
        {
            var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
            return new InternalAzureKeyVaultClient(TokenCredentialFactory.Create(), secretRepositoryOptions, NullLogger<InternalAzureKeyVaultClient>.Instance);
        }
    }
}