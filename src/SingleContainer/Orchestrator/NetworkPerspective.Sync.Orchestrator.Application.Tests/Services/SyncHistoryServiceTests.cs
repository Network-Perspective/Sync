using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Application.Tests;
using NetworkPerspective.Sync.Utils.Models;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services;

public class SyncHistoryServiceTests
{
    private readonly ICryptoService _cryptoService = new CryptoService();
    private readonly IClock _clock = new Clock();
    private static readonly ILogger<SyncHistoryService> NullLogger = NullLogger<SyncHistoryService>.Instance;
    private readonly Mock<IWorkerRouter> _workerRouterMock = new();

    [Fact]
    public async Task ShouldReturnEndOfLastSyncPeriod()
    {
        // Arrange
        const string workerName = "worker-name";
        using var uowFactory = new SqliteUnitOfWorkFactory();

        _workerRouterMock
            .Setup(x => x.IsConnected(workerName))
            .Returns(true);

        var workersService = new WorkersService(uowFactory.Create(), _workerRouterMock.Object, new Clock(), _cryptoService, NullLogger<WorkersService>.Instance);
        var workerId = await workersService.CreateAsync(workerName, "secret");

        var connectorId = Guid.NewGuid();
        var networkId = Guid.NewGuid();
        var type = "Slack";
        var properties = new Dictionary<string, string>
            {
                { "StringProp", "some-prop" },
                { "BoolProp", "true" },
                { "IntProp", "321" }
            };
        var connectorService = new ConnectorsService(uowFactory.Create(), _clock, NullLogger<ConnectorsService>.Instance);
        await connectorService.CreateAsync(connectorId, networkId, type, workerId, properties);

        var syncStart = DateTime.UtcNow;
        var syncEnd = DateTime.UtcNow.AddDays(1);
        var record = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(syncStart, syncEnd), 42, 2, 4242);

        var service = new SyncHistoryService(uowFactory.Create(), _clock, NullLogger);
        await service.SaveLogAsync(record);

        // Act
        var result = await service.EvaluateSyncStartAsync(connectorId);

        // Aseert
        result.Should().Be(syncEnd);
    }

    [Fact]
    public async Task ShouldReturnSyncStartTime()
    {
        // Arrange
        const string workerName = "worker-name";
        using var uowFactory = new SqliteUnitOfWorkFactory();

        _workerRouterMock
            .Setup(x => x.IsConnected(workerName))
            .Returns(true);

        var workersService = new WorkersService(uowFactory.Create(), _workerRouterMock.Object, new Clock(), _cryptoService, NullLogger<WorkersService>.Instance);
        var workerId = await workersService.CreateAsync(workerName, "secret");

        var connectorId = Guid.NewGuid();
        var networkId = Guid.NewGuid();
        var type = "Slack";
        var properties = new Dictionary<string, string>
            {
                { "StringProp", "some-prop" },
                { "BoolProp", "true" },
                { "IntProp", "321" }
            };
        var connectorService = new ConnectorsService(uowFactory.Create(), _clock, NullLogger<ConnectorsService>.Instance);
        await connectorService.CreateAsync(connectorId, networkId, type, workerId, properties);

        var now = DateTime.UtcNow;
        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(x => x.UtcNow())
            .Returns(now);


        var service = new SyncHistoryService(uowFactory.Create(), clockMock.Object, NullLogger);

        // Act
        var result = await service.EvaluateSyncStartAsync(Guid.NewGuid());

        // Aseert
        result.Should().Be(now);
    }

    [Fact]
    public async Task ShouldOverrideSyncStart()
    {
        // Arrange
        const string workerName = "worker-name";
        using var uowFactory = new SqliteUnitOfWorkFactory();

        _workerRouterMock
            .Setup(x => x.IsConnected(workerName))
            .Returns(true);

        var workersService = new WorkersService(uowFactory.Create(), _workerRouterMock.Object, new Clock(), _cryptoService, NullLogger<WorkersService>.Instance);
        var workerId = await workersService.CreateAsync(workerName, "secret");

        var connectorId = Guid.NewGuid();
        var networkId = Guid.NewGuid();
        var type = "Slack";
        var properties = new Dictionary<string, string>
            {
                { "StringProp", "some-prop" },
                { "BoolProp", "true" },
                { "IntProp", "321" }
            };
        var connectorService = new ConnectorsService(uowFactory.Create(), _clock, NullLogger<ConnectorsService>.Instance);
        await connectorService.CreateAsync(connectorId, networkId, type, workerId, properties);

        var syncStart = DateTime.UtcNow;
        var syncEnd = DateTime.UtcNow.AddDays(1);
        var overridenSyncStart = new DateTime(2020, 01, 01);

        var record = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(syncStart, syncEnd), 0, 0, 0);

        var service = new SyncHistoryService(uowFactory.Create(), _clock, NullLogger);
        await service.SaveLogAsync(record);

        await service.OverrideSyncStartAsync(connectorId, overridenSyncStart);

        // Act
        var result = await service.EvaluateSyncStartAsync(connectorId);

        // Aseert
        result.Should().Be(overridenSyncStart);
    }
}