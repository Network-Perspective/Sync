using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Slack;
using NetworkPerspective.Sync.Slack.Client;
using NetworkPerspective.Sync.Slack.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Slack.Tests
{
    [Collection(SlackTestsCollection.Name)]
    public class BaseTest
    {
        private readonly InMemoryHostedServiceFixture<Startup> _service;

        public BaseTest(InMemoryHostedServiceFixture<Startup> service)
        {
            _service = service;
            service.Reset();
        }

        [Fact]
        public async Task ShouldReturn401OnInvalidToken()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            var networkConfig = new NetworkConfigDto
            {
                AutoJoinChannels = true,
                SyncChannelsNames = true,
                UsesAdminPrivileges = true
            };

            // Act
            Func<Task> func = () => new NetworksClient(httpClient).NetworksPostAsync(networkConfig);

            // Assert
            await func.Should().ThrowAsync<SlackClientException>().Where(x => x.StatusCode == 401);
        }

        [Fact]
        public async Task ShouldSetupNetworkProperly()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            const bool autoJoinChannels = true;
            const bool syncChannelsNames = true;
            const bool usesAdminPrivileges = true;

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectorInfo(networkId, Guid.NewGuid()));

            var networkConfig = new NetworkConfigDto
            {
                AutoJoinChannels = autoJoinChannels,
                SyncChannelsNames = syncChannelsNames,
                UsesAdminPrivileges = usesAdminPrivileges
            };

            // Act
            var result = await new NetworksClient(httpClient)
                .NetworksPostAsync(networkConfig);

            // Assert
            _service.SecretRepositoryMock.Verify(x => x.SetSecretAsync($"np-token-{networkId}", It.Is<SecureString>(x => x.ToSystemString() == _service.ValidToken), It.IsAny<CancellationToken>()), Times.Once);

            using var unitOfWork = _service.UnitOfWorkFactory.Create();
            var networksRepository = unitOfWork.GetConnectorRepository<SlackConnectorProperties>();
            var network = await networksRepository.FindAsync(networkId);
            network.Properties.AutoJoinChannels.Should().Be(autoJoinChannels);
            network.Properties.SyncGroups.Should().Be(syncChannelsNames);
            network.Properties.ExternalKeyVaultUri.Should().BeNull();
            network.Properties.UsesAdminPrivileges.Should().Be(usesAdminPrivileges);
        }

        [Fact]
        public async Task ShouldSetupSchedulesProperly()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            const bool autoJoinChannels = true;
            const bool syncChannelsNames = true;

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectorInfo(networkId, Guid.NewGuid()));

            var networkConfig = new NetworkConfigDto
            {
                AutoJoinChannels = autoJoinChannels,
                SyncChannelsNames = syncChannelsNames
            };

            await new NetworksClient(httpClient)
                .NetworksPostAsync(networkConfig);

            // Act
            var result = await new SchedulesClient(httpClient)
                .SchedulesPostAsync(new SchedulerStartDto());

            // Assert
            var status = await new StatusClient(httpClient)
                .StatusAsync();
            status.Scheduled.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnError()
        {
            // Arrange
            var httpClient = _service.CreateDefaultClient();
            var client = new NetworksClient(httpClient);

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidTokenException("https://networkperspective.io/"));

            var networkConfig = new NetworkConfigDto
            {
                AutoJoinChannels = true,
                SyncChannelsNames = true
            };

            // Act
            var exception = await Record.ExceptionAsync(() => client.NetworksPostAsync(networkConfig));

            // Assert
            (exception as SlackClientException).StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
    }
}