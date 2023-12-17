using System;
using System.Threading;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Moq;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests.Factories;

using NetworkPerspective.Sync.Infrastructure.Microsoft.Configs;

using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures
{
    internal static class ClientFactory
    {
        public static GraphServiceClient Create(bool syncMsTeams)
        {
            var networkId = new Guid("bd1bc916-db78-4e1e-b93b-c6feb8cf729e");
            var secretRepositoryFactory = new TestableAzureKeyVaultClientFactory();

            var resiliency = Options.Create(
                new Resiliency
                {
                    Retries = new[] { TimeSpan.FromMilliseconds(100) }
                });

            var networkProperties = new MicrosoftNetworkProperties(syncMsTeams, null);
            var network = Network<MicrosoftNetworkProperties>.Create(networkId, networkProperties, DateTime.UtcNow);
            var networkService = new Mock<INetworkService>();
            networkService
                .Setup(x => x.GetAsync<MicrosoftNetworkProperties>(networkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(network);

            var microsoftClientFactory = new MicrosoftClientFactory(secretRepositoryFactory, networkService.Object, resiliency, NullLoggerFactory.Instance);
            return microsoftClientFactory.GetMicrosoftClientAsync(networkId).Result;
        }
    }
}
