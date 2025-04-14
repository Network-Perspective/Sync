using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;
using NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.UseCases.Handlers;

public class SyncHandlerTests
{
    private readonly Mock<ISyncContextFactory> _syncContextFactoryMock = new();
    private readonly Mock<ISyncService> _syncServiceMock = new();
    private readonly ILogger<SyncHandler> _logger = NullLogger<SyncHandler>.Instance;

    public SyncHandlerTests()
    {
        _syncContextFactoryMock.Reset();
        _syncServiceMock.Reset();
    }

    [Fact]
    public async Task ShouldSetEmptyCurrentTaskOnException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var tasksStatusCache = new GlobalStatusCache();
        var syncContextAccessor = new SyncContextAccessor();

        _syncServiceMock
            .Setup(x => x.SyncAsync(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception());

        var handler = new SyncHandler(_syncContextFactoryMock.Object, syncContextAccessor, _syncServiceMock.Object, tasksStatusCache, _logger);

        var request = new SyncRequest
        {
            Connector = new ConnectorDto
            {
                Id = connectorId,
            }
        };

        // Act
        try
        {
            await handler.HandleAsync(request);
        }
        catch (Exception) { }

        // Assert
        var status = await tasksStatusCache.GetStatusAsync(connectorId);
        Assert.Equal(SingleTaskStatus.Empty, status);
    }

    [Fact]
    public async Task ShouldSetCurrentTaskToEmptyOnCompleted()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var tasksStatusCache = new GlobalStatusCache();
        var syncContextAccessor = new SyncContextAccessor();

        _syncServiceMock
            .Setup(x => x.SyncAsync(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncResult(42, 4242, []));

        var handler = new SyncHandler(_syncContextFactoryMock.Object, syncContextAccessor, _syncServiceMock.Object, tasksStatusCache, _logger);

        var request = new SyncRequest
        {
            CorrelationId = connectorId,
            Start = DateTime.UtcNow.AddDays(-1),
            End = DateTime.UtcNow,
            Connector = new ConnectorDto
            {
                Id = connectorId,
            }
        };

        // Act
        await handler.HandleAsync(request);

        // Assert
        var status = await tasksStatusCache.GetStatusAsync(connectorId);
        Assert.Equal(SingleTaskStatus.Empty, status);
    }

    [Fact]
    public async Task ShouldBlockRunningConcurrentSyncForConnector()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var tasksStatusCache = new GlobalStatusCache();
        var syncContextAccessor = new SyncContextAccessor();

        var request = new SyncRequest
        {
            CorrelationId = connectorId,
            Start = DateTime.UtcNow.AddDays(-1),
            End = DateTime.UtcNow,
            Connector = new ConnectorDto
            {
                Id = connectorId,
            }
        };

        _syncServiceMock
            .SetupSequence(x => x.SyncAsync(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(10000);
                return new SyncResult(42, 4242, []);
            })
            .ReturnsAsync(new SyncResult(42, 4242, []));

        var handler1 = new SyncHandler(_syncContextFactoryMock.Object, syncContextAccessor, _syncServiceMock.Object, tasksStatusCache, _logger);
        var handler2 = new SyncHandler(_syncContextFactoryMock.Object, syncContextAccessor, _syncServiceMock.Object, tasksStatusCache, _logger);

        _ = handler1.HandleAsync(request);

        // Act Assert
        var ex = await Assert.ThrowsAsync<SyncAlreadyInProgressException>(async () => await handler2.HandleAsync(request));
    }

    [Fact]
    public async Task ShouldAllowSyncAfterFirstIsCompleted()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var tasksStatusCache = new GlobalStatusCache();
        var syncContextAccessor = new SyncContextAccessor();

        var request = new SyncRequest
        {
            CorrelationId = connectorId,
            Start = DateTime.UtcNow.AddDays(-1),
            End = DateTime.UtcNow,
            Connector = new ConnectorDto
            {
                Id = connectorId,
            }
        };

        _syncServiceMock
            .Setup(x => x.SyncAsync(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncResult(42, 4242, []));

        var handler1 = new SyncHandler(_syncContextFactoryMock.Object, syncContextAccessor, _syncServiceMock.Object, tasksStatusCache, _logger);
        var handler2 = new SyncHandler(_syncContextFactoryMock.Object, syncContextAccessor, _syncServiceMock.Object, tasksStatusCache, _logger);

        // Act
        var result1 = await handler1.HandleAsync(request);
        var result2 = await handler2.HandleAsync(request);

        // Assert
        Assert.Equal(result1.ConnectorId, connectorId);
        Assert.Equal(result2.ConnectorId, connectorId);
    }

    [Fact]
    public async Task ShouldAllowToRunConcurrentSyncForDifferentConnectors()
    {
        // Arrange
        var connectorId1 = Guid.NewGuid();
        var connectorId2 = Guid.NewGuid();
        var tasksStatusCache = new GlobalStatusCache();
        var syncContextAccessor = new SyncContextAccessor();

        var request1 = new SyncRequest
        {
            CorrelationId = connectorId1,
            Start = DateTime.UtcNow.AddDays(-1),
            End = DateTime.UtcNow,
            Connector = new ConnectorDto
            {
                Id = connectorId1,
            }
        };

        var request2 = new SyncRequest
        {
            CorrelationId = connectorId2,
            Start = DateTime.UtcNow.AddDays(-1),
            End = DateTime.UtcNow,
            Connector = new ConnectorDto
            {
                Id = connectorId2,
            }
        };

        _syncServiceMock
            .SetupSequence(x => x.SyncAsync(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(10000);
                return new SyncResult(42, 4242, []);
            })
            .ReturnsAsync(new SyncResult(42, 4242, []));

        var handler1 = new SyncHandler(_syncContextFactoryMock.Object, syncContextAccessor, _syncServiceMock.Object, tasksStatusCache, _logger);
        var handler2 = new SyncHandler(_syncContextFactoryMock.Object, syncContextAccessor, _syncServiceMock.Object, tasksStatusCache, _logger);

        _ = handler1.HandleAsync(request1);

        // Act
        var result = await handler2.HandleAsync(request2);

        // Arrange
        Assert.Equal(connectorId2, result.ConnectorId);
    }
}