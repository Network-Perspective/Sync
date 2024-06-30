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
using NetworkPerspective.Sync.GSuite.Client;
using NetworkPerspective.Sync.GSuite.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Google;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.GSuite.Tests
{
    [Collection(GSuiteTestsCollection.Name)]
    public class BaseTests
    {
        private readonly InMemoryHostedServiceFixture<Startup> _service;

        public BaseTests(InMemoryHostedServiceFixture<Startup> service)
        {
            _service = service;
            service.Reset();
        }

        [Fact]
        public async Task ShouldReturn401OnInvalidToken()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            const string adminEmail = "admin@networkperspective.io";

            var networkConfig = new NetworkConfigDto
            {
                AdminEmail = adminEmail
            };

            // Act
            Func<Task> func = () => new NetworksClient(httpClient).NetworksPostAsync(networkConfig);

            // Assert
            await func.Should().ThrowAsync<GSuiteClientException>().Where(x => x.StatusCode == 401);
        }

        [Fact]
        public async Task ShouldSetupNetworkProperly()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            const string adminEmail = "admin@networkperspective.io";

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectorInfo(connectorId, Guid.NewGuid()));

            var networkConfig = new NetworkConfigDto
            {
                AdminEmail = adminEmail
            };

            // Act
            var result = await new NetworksClient(httpClient)
                .NetworksPostAsync(networkConfig);

            // Assert
            _service.SecretRepositoryMock.Verify(x => x.SetSecretAsync($"np-token-GSuite-{connectorId}", It.Is<SecureString>(x => x.ToSystemString() == _service.ValidToken), It.IsAny<CancellationToken>()), Times.Once);

            using var unitOfWork = _service.UnitOfWorkFactory.Create();
            var networksRepository = unitOfWork.GetConnectorRepository<GoogleNetworkProperties>();
            var network = await networksRepository.FindAsync(connectorId);
            network.Properties.AdminEmail.Should().Be(adminEmail);
            network.Properties.ExternalKeyVaultUri.Should().BeNull();
        }

        [Fact]
        public async Task ShouldSetupSchedulesProperly()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            const string adminEmail = "admin@networkperspective.io";

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectorInfo(connectorId, Guid.NewGuid()));

            var networkConfig = new NetworkConfigDto
            {
                AdminEmail = adminEmail
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
                AdminEmail = "foo@networkperspective.io"
            };

            // Act
            var exception = await Record.ExceptionAsync(() => client.NetworksPostAsync(networkConfig));

            // Assert
            (exception as GSuiteClientException).StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
    }
}