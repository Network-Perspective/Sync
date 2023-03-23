using System;

using Microsoft.Graph;

using NetworkPerspective.Sync.Common.Tests.Factories;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures
{
    public class MicrosoftClientFixture
    {
        public GraphServiceClient GraphServiceClient { get; }

        public MicrosoftClientFixture()
        {
            var networkId = new Guid("bd1bc916-db78-4e1e-b93b-c6feb8cf729e");
            var secretRepositoryFactory = new TestableAzureKeyVaultClientFactory();
            var microsoftClientFactory = new MicrosoftClientFactory(secretRepositoryFactory);
            GraphServiceClient = microsoftClientFactory.GetMicrosoftClientAsync(networkId).Result;
        }
    }
}