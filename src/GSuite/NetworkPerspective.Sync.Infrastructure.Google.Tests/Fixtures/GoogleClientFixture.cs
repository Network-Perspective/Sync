using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Google.Services;
using NetworkPerspective.Sync.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Fixtures
{
    public class GoogleClientFixture
    {
        public GoogleCredential Credential { get; }

        public GoogleClientFixture()
        {
            var secretRepositoryOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = TestsConsts.InternalAzureKeyVaultBaseUrl });
            var secretRepository = new InternalAzureKeyVaultClient(TokenCredentialFactory.Create(), secretRepositoryOptions, NullLogger<InternalAzureKeyVaultClient>.Instance);
            var credentialProvider = new CredentialsProvider(secretRepository);
            Credential = credentialProvider.GetCredentialsAsync().Result;
        }
    }
}