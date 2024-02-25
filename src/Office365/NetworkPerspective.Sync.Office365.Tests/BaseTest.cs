using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Microsoft;
using NetworkPerspective.Sync.Office365.Client;
using NetworkPerspective.Sync.Office365.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Office365.Tests
{
    [Collection(Office365TestsCollection.Name)]
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

            var networkConfig = new NetworkConfigDto();

            // Act
            Func<Task> func = () => new NetworksClient(httpClient).NetworksPostAsync(networkConfig);

            // Assert
            await func.Should().ThrowAsync<Office365ClientException>().Where(x => x.StatusCode == 401);
        }

        [Fact]
        public async Task ShouldSetupNetworkProperly()
        {
            // Arrange
            var syncMsTeams = true;
            var networkId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenValidationResponse(networkId, Guid.NewGuid()));

            var networkConfig = new NetworkConfigDto
            {
                SyncMsTeams = syncMsTeams
            };

            // Act
            var result = await new NetworksClient(httpClient)
                .NetworksPostAsync(networkConfig);

            // Assert
            _service.SecretRepositoryMock.Verify(x => x.SetSecretAsync($"np-token-Office365-{networkId}", It.Is<SecureString>(x => x.ToSystemString() == _service.ValidToken), It.IsAny<CancellationToken>()), Times.Once);

            using var unitOfWork = _service.UnitOfWorkFactory.Create();
            var networksRepository = unitOfWork.GetNetworkRepository<MicrosoftNetworkProperties>();
            var network = await networksRepository.FindAsync(networkId);
            network.Properties.ExternalKeyVaultUri.Should().BeNull();
            network.Properties.SyncMsTeams.Should().Be(syncMsTeams);
        }

        [Fact]
        public async Task ShouldSetupSchedulesProperly()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            _service.SecretRepositoryMock
                .Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString().ToSecureString());

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenValidationResponse(networkId, Guid.NewGuid()));

            var networkConfig = new NetworkConfigDto
            {
                SyncMsTeams = false
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
                SyncMsTeams = false
            };

            // Act
            var exception = await Record.ExceptionAsync(() => client.NetworksPostAsync(networkConfig));

            // Assert
            (exception as Office365ClientException).StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
    }
}