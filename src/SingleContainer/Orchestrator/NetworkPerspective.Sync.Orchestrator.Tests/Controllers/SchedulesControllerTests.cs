using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Controllers;
using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

using Xunit;

namespace NetworkPerspective.Sync.Framework.Tests.Controllers;

public class SchedulesControllerTests
{
    private readonly Mock<ISyncScheduler> _schduleFacadeMock = new();
    private readonly Mock<IConnectorsService> _connectorsServiceMock = new();
    private readonly Mock<ISyncHistoryService> _syncHistoryServiceMock = new();
    private readonly Mock<IStatusLogger> _statusLoggerMock = new();

    public SchedulesControllerTests()
    {
        _schduleFacadeMock.Reset();
        _syncHistoryServiceMock.Reset();
        _statusLoggerMock.Reset();
    }

    public class Start : SchedulesControllerTests
    {
        [Fact]
        public async Task ShouldAddToSchedulerOnValidToken()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            var controller = Create();

            // Act
            await controller.StartAsync(connectorId, new SchedulerStartDto());

            // Assert
            _schduleFacadeMock.Verify(x => x.ScheduleAsync(connectorId, It.IsAny<CancellationToken>()), Times.Once);
            _syncHistoryServiceMock.Verify(x => x.OverrideSyncStartAsync(connectorId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ShouldTriggerSyncOnValidToken()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            var controller = Create();

            // Act
            await controller.StartAsync(connectorId, new SchedulerStartDto());

            // Assert
            _schduleFacadeMock.Verify(x => x.TriggerNowAsync(connectorId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ShouldNotAddToSchedulerOnNonExistingNetwork()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            _connectorsServiceMock
                .Setup(x => x.ValidateExists(connectorId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConnectorNotFoundException(connectorId));

            var controller = Create();
            Func<Task> func = () => controller.StartAsync(connectorId, new SchedulerStartDto());

            // Act Assert
            await func.Should().ThrowExactlyAsync<ConnectorNotFoundException>();
            _schduleFacadeMock.Verify(x => x.ScheduleAsync(connectorId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ShouldOverrideSyncPeriodStart()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var syncPeriodStart = DateTime.Now;

            var controller = Create();

            // Act
            await controller.StartAsync(connectorId, new SchedulerStartDto { OverrideSyncPeriodStart = syncPeriodStart });

            // Assert
            _syncHistoryServiceMock.Verify(x => x.OverrideSyncStartAsync(connectorId, syncPeriodStart.ToUniversalTime(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class Stop : SchedulesControllerTests
    {
        [Fact]
        public async Task ShouldRemoveFromSchedulerOnValidToken()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            var controller = Create();

            // Act
            await controller.StopAsync(connectorId);

            // Assert
            _schduleFacadeMock.Verify(x => x.UnscheduleAsync(connectorId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    protected SchedulesController Create()
    {
        var controller = new SchedulesController(_schduleFacadeMock.Object, _connectorsServiceMock.Object, _syncHistoryServiceMock.Object, _statusLoggerMock.Object);

        return controller;
    }
}