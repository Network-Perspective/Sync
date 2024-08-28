using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Mapster;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Client;
using NetworkPerspective.Sync.Orchestrator.Controllers;
using NetworkPerspective.Sync.Orchestrator.Mappers;

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
            ControllersMapsterConfig.RegisterMappings(TypeAdapterConfig.GlobalSettings);

            var connectorId = Guid.NewGuid();
            var isConnected = true;
            var isScheduled = true;
            var isAuthorized = true;
            var isRunning = true;
            var caption = "caption";
            var description = "description";
            var completionRate = 42.42;

            var taskStatus = ConnectorTaskStatus.Create(caption, description, completionRate);
            var status = Status.Connected(isScheduled, ConnectorStatus.Running(isAuthorized, taskStatus), []);

            _statusServiceMock
                .Setup(x => x.GetStatusAsync(connectorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(status);

            var controller = new StatusController(_statusServiceMock.Object);

            // Act
            var actualStatus = await controller.GetStatus(connectorId);

            // Assert
            var expectedStatus = new StatusDto
            {
                IsConnected = isConnected,
                Scheduled = isScheduled,
                Authorized = isAuthorized,
                Running = isRunning,
                CurrentTask = new SynchronizationTaskStatusDto()
                {
                    Caption = caption,
                    Description = description,
                    CompletionRate = completionRate
                },
                Logs = []
            };
            actualStatus.Should().BeEquivalentTo(expectedStatus);
        }
    }
}