using System;
using System.Threading;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Connectors;
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
            var connectorId = new Guid("04C753D8-FF9A-479C-B857-5D28C1EAF6C1");
            var secretRepositoryFactory = new TestableAzureKeyVaultClientFactory();
            var secretRepository = new TestableAzureKeyVaultClientFactory().CreateAsync(connectorId).Result;

            var resiliency = Options.Create(
                new Resiliency
                {
                    Retries = new[] { TimeSpan.FromMilliseconds(100) }
                });

            var networkProperties = new MicrosoftNetworkProperties(syncMsTeams, true, true, true, null);
            var network = Connector<MicrosoftNetworkProperties>.Create(connectorId, networkProperties, DateTime.UtcNow);
            var networkService = new Mock<IConnectorService>();
            networkService
                .Setup(x => x.GetAsync<MicrosoftNetworkProperties>(connectorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(network);

            var contextProviderMock = new Mock<IConnectorInfoProvider>();
            contextProviderMock
                .Setup(x => x.Get())
                .Returns(new ConnectorInfo(connectorId, networkId));

            var microsoftClientFactory = new MicrosoftClientFactory(secretRepository, contextProviderMock.Object, networkService.Object, resiliency, NullLoggerFactory.Instance);
            return microsoftClientFactory.GetMicrosoftClientAsync().Result;
        }
    }
}