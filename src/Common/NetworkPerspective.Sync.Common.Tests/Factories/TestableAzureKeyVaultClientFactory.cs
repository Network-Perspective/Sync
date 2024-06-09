using System;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Common.Tests.Factories
{
    public class TestableAzureKeyVaultClientFactory : ISecretRepositoryFactory
    {
        public ISecretRepository Create(Uri externalKeyVaultUri = default)
        {
            var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
            var secretRepository = new InternalAzureKeyVaultClient(TokenCredentialFactory.Create(), secretRepositoryOptions, NullLogger<InternalAzureKeyVaultClient>.Instance);
            return secretRepository;
        }
    }
}