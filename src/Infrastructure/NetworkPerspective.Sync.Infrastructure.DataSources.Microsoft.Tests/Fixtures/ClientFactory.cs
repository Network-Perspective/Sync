using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

using Moq;

using NetworkPerspective.Sync.Common.Tests.Factories;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Fixtures
{
    internal static class ClientFactory
    {
        public static GraphServiceClient Create(bool syncMsTeams)
        {
            var networkId = new Guid("bd1bc916-db78-4e1e-b93b-c6feb8cf729e");
            var connectorId = new Guid("04C753D8-FF9A-479C-B857-5D28C1EAF6C1");
            var secretRepository = TestableAzureKeyVaultClient.Create();

            var resiliency = Options.Create(
                new Resiliency
                {
                    Retries = [TimeSpan.FromMilliseconds(100)]
                });

            var networkProperties = new MicrosoftNetworkProperties(syncMsTeams, true, true, true, null);
            var network = Connector<MicrosoftNetworkProperties>.Create(connectorId, networkProperties, DateTime.UtcNow);
            var hashingService = new Mock<IHashingService>();
            hashingService
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns<string>(x => x);

            var connectorInfo = new ConnectorContext(connectorId, "Office365", new Dictionary<string, string>());

            var connectorInforProviederMock = new Mock<IConnectorContextAccessor>();
            connectorInforProviederMock
                .Setup(x => x.Get())
                .Returns(connectorInfo);

            var microsoftClientFactory = new MicrosoftClientFactory(secretRepository, connectorInforProviederMock.Object, resiliency, NullLoggerFactory.Instance);
            return microsoftClientFactory.GetMicrosoftClientAsync().Result;
        }
    }
}