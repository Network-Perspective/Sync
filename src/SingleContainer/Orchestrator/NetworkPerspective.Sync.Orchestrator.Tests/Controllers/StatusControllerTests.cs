using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Controllers;

using Xunit;

namespace NetworkPerspective.Sync.Framework.Tests.Controllers;

public class StatusControllerTests
{
    private readonly Mock<IStatusService> _statusServiceMock = new();

    public StatusControllerTests()
    {
        _statusServiceMock.Reset();
    }

    public class GetStatus : StatusControllerTests
    {
        [Fact]
        public async Task ShouldReturnStatus()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();

            var status = new Status
            {
                Authorized = false,
                Running = false,
                Scheduled = false,
                Logs = []
            };

            _statusServiceMock
                .Setup(x => x.GetStatusAsync(connectorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(status);

            var controller = Create();

            // Act
            var result = await controller.GetStatus(connectorId);

            // Assert
            result.Should().BeEquivalentTo(status);
        }

        //[Fact]
        //public async Task ShoulThrowConnectorNotFoundOnNonExistingConnector()
        //{
        //    // Arrange
        //    var networkId = Guid.NewGuid();
        //    var connectorId = Guid.NewGuid();
        //    var accessToken = "access-token";

        //    var controller = Create(accessToken);
        //    Func<Task> func = () => controller.GetStatus(connectorId);

        //    // Act Assert
        //    await func.Should().ThrowExactlyAsync<ConnectorNotFoundException>();
        //    _statusServiceMock.Verify(x => x.GetStatusAsync(connectorId, It.IsAny<CancellationToken>()), Times.Never);
        //}

        // todo - implement full blown status service and adjust
    }

    private StatusController Create()
    {
        var controller = new StatusController(_statusServiceMock.Object);

        return controller;
    }
}