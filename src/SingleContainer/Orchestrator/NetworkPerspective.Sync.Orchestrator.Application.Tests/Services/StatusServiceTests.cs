using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Services;

public class StatusServiceTests
{
    private readonly ILogger<StatusService> _logger = NullLogger<StatusService>.Instance;
    private readonly Mock<IWorkerRouter> _workerRouterMock = new();
    private readonly Mock<ISyncScheduler> _schedulerMock = new();

    public StatusServiceTests()
    {
        _workerRouterMock.Reset();
    }

    [Fact]
    public async Task ShouldThrowOnConnectorNotExists()
    {
        // Arrange
        using var uowFactory = new SqliteUnitOfWorkFactory();

        var service = new StatusService(_workerRouterMock.Object, uowFactory.Create(), _schedulerMock.Object, _logger);

        Func<Task<Status>> func = () => service.GetStatusAsync(Guid.NewGuid());

        // Act Assert
        await func.Should().ThrowExactlyAsync<EntityNotFoundException<Connector>>();
    }

    [Fact]
    public async Task ShouldReturnDisconnectedStatus()
    {
        // Arrange
        using var uowFactory = new SqliteUnitOfWorkFactory();

        var worker = new Worker(Guid.NewGuid(), 1, "worker-name", "hash", "salt", true, DateTime.UtcNow);

        var unitOfWork = uowFactory.Create();
        await unitOfWork
            .GetWorkerRepository()
            .AddAsync(worker);

        var connector = new Connector(Guid.NewGuid(), "Slack", new Dictionary<string, string>(), worker, Guid.NewGuid(), DateTime.UtcNow);
        await unitOfWork
            .GetConnectorRepository()
            .AddAsync(connector);

        var log1 = StatusLog.Create(connector.Id, "Message - info", StatusLogLevel.Info, DateTime.UtcNow);
        var log2 = StatusLog.Create(connector.Id, "Message - warning", StatusLogLevel.Warning, DateTime.UtcNow);
        var log3 = StatusLog.Create(connector.Id, "Message - error", StatusLogLevel.Error, DateTime.UtcNow);
        var logsRepo = unitOfWork.GetStatusLogRepository();
        await logsRepo.AddAsync(log1);
        await logsRepo.AddAsync(log2);
        await logsRepo.AddAsync(log3);

        await unitOfWork.CommitAsync();

        _workerRouterMock
            .Setup(x => x.IsConnected(worker.Name))
            .Returns(false);

        _schedulerMock
            .Setup(x => x.IsScheduledAsync(connector.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new StatusService(_workerRouterMock.Object, uowFactory.Create(), _schedulerMock.Object, _logger);

        // Act
        var status = await service.GetStatusAsync(connector.Id);

        // Assert
        status.WorkerStatus.IsConnected.Should().BeFalse();
        status.WorkerStatus.IsScheduled.Should().BeTrue();
        status.ConnectorStatus.Should().BeEquivalentTo(ConnectorStatus.Unknown);
        status.Logs.Should().BeEquivalentTo([log1, log2, log3]);
    }

    [Fact]
    public async Task ShouldReturnConnectedStatus()
    {
        // Arrange
        using var uowFactory = new SqliteUnitOfWorkFactory();

        var worker = new Worker(Guid.NewGuid(), 1, "worker-name", "hash", "salt", true, DateTime.UtcNow);

        var unitOfWork = uowFactory.Create();
        await unitOfWork
            .GetWorkerRepository()
            .AddAsync(worker);

        var connector = new Connector(Guid.NewGuid(), "Slack", new Dictionary<string, string>(), worker, Guid.NewGuid(), DateTime.UtcNow);
        await unitOfWork
            .GetConnectorRepository()
            .AddAsync(connector);

        var log1 = StatusLog.Create(connector.Id, "Message - info", StatusLogLevel.Info, DateTime.UtcNow);
        var log2 = StatusLog.Create(connector.Id, "Message - warning", StatusLogLevel.Warning, DateTime.UtcNow);
        var log3 = StatusLog.Create(connector.Id, "Message - error", StatusLogLevel.Error, DateTime.UtcNow);
        var logsRepo = unitOfWork.GetStatusLogRepository();
        await logsRepo.AddAsync(log1);
        await logsRepo.AddAsync(log2);
        await logsRepo.AddAsync(log3);

        await unitOfWork.CommitAsync();

        _workerRouterMock
            .Setup(x => x.IsConnected(worker.Name))
            .Returns(true);

        var currentTask = new ConnectorTaskStatus("caption", "description", 33);
        var connectorStatus = ConnectorStatus.Running(true, currentTask);
        _workerRouterMock
            .Setup(x => x.GetConnectorStatusAsync(worker.Name, connector.Id, connector.NetworkId, connector.Properties, connector.Type))
            .ReturnsAsync(connectorStatus);

        _schedulerMock
            .Setup(x => x.IsScheduledAsync(connector.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new StatusService(_workerRouterMock.Object, uowFactory.Create(), _schedulerMock.Object, _logger);

        // Act
        var status = await service.GetStatusAsync(connector.Id);

        // Assert
        status.WorkerStatus.IsConnected.Should().BeTrue();
        status.WorkerStatus.IsScheduled.Should().BeFalse();
        status.ConnectorStatus.Should().BeEquivalentTo(connectorStatus);
        status.Logs.Should().BeEquivalentTo([log1, log2, log3]);
    }
}