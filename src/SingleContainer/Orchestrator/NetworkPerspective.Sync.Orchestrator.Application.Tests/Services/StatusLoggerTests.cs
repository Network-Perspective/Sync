using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Services;

public class StatusLoggerTests
{
    private readonly ICryptoService _cryptoService = new CryptoService();
    private readonly IClock _clock = new Clock();
    private static readonly ILogger<StatusLogger> NullLogger = NullLogger<StatusLogger>.Instance;
    private readonly Mock<IWorkerRouter> _workerRouterMock = new();

    [Fact]
    public async Task ShouldReturnPersistedLogs()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        const string workerName = "worker-name";

        using var uowFactory = new SqliteUnitOfWorkFactory();

        _workerRouterMock
            .Setup(x => x.IsConnected(workerName))
            .Returns(true);

        var workersService = new WorkersService(uowFactory.Create(), _workerRouterMock.Object, new Clock(), _cryptoService, NullLogger<WorkersService>.Instance);
        await workersService.CreateAsync(workerId, workerName, "secret");

        var connectorId = Guid.NewGuid();
        var networkId = Guid.NewGuid();
        var type = "Slack";
        var properties = new Dictionary<string, string>();
        var connectorService = new ConnectorsService(uowFactory.Create(), _clock, NullLogger<ConnectorsService>.Instance);
        await connectorService.CreateAsync(connectorId, networkId, type, workerId, properties);

        const string message1 = "Dummy message Error";
        const string message2 = "Dummy message Info";
        var timeStamp1 = DateTime.UtcNow;
        var timeStamp2 = DateTime.UtcNow;

        var clockMock = new Mock<IClock>();
        clockMock
            .SetupSequence(x => x.UtcNow())
            .Returns(timeStamp1)
            .Returns(timeStamp2);

        var statusLogger = new StatusLogger(uowFactory, clockMock.Object, NullLogger);

        // Act
        await statusLogger.AddLogAsync(connectorId, message1, StatusLogLevel.Error);
        await statusLogger.AddLogAsync(connectorId, message2, StatusLogLevel.Info);
        await statusLogger.AddLogAsync(connectorId, message2, StatusLogLevel.Debug);

        // Assert
        var result = await uowFactory
            .Create()
            .GetStatusLogRepository()
            .GetListAsync(connectorId, StatusLogLevel.Info);

        Assert.Equal(2, result.Count());

        var log1 = result.First(x => x.Level == StatusLogLevel.Error);
        Assert.Equal(connectorId, log1.ConnectorId);
        Assert.Equal(message1, log1.Message);
        Assert.Equal(StatusLogLevel.Error, log1.Level);
        Assert.Equal(timeStamp1, log1.TimeStamp);

        var log2 = result.First(x => x.Level == StatusLogLevel.Info);
        Assert.Equal(connectorId, log2.ConnectorId);
        Assert.Equal(message2, log2.Message);
        Assert.Equal(StatusLogLevel.Info, log2.Level);
        Assert.Equal(timeStamp2, log2.TimeStamp);
    }

    [Fact]
    public async Task ShouldCatchException()
    {
        // Arrange
        using var unitOfWorkFactory = new SqliteUnitOfWorkFactory();

        var statusLogger = new StatusLogger(unitOfWorkFactory, new Clock(), NullLogger);

        var excetion = await Record.ExceptionAsync(() => statusLogger.AddLogAsync(Guid.NewGuid(), "Dummy message Error", StatusLogLevel.Error));

        // Act Assert
        Assert.Null(excetion);
    }
}