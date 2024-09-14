using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Services;

public class StatusLoggerTests
{
    private readonly ICryptoService _cryptoService = new CryptoService();
    private readonly IClock _clock = new Clock();
    private static readonly ILogger<StatusLogger> NullLogger = NullLogger<StatusLogger>.Instance;

    [Fact]
    public async Task ShouldReturnPersistedLogs()
    {
        // Arrange
        using var uowFactory = new SqliteUnitOfWorkFactory();

        var workersService = new WorkersService(uowFactory.Create(), new Clock(), _cryptoService, NullLogger<WorkersService>.Instance);
        var workerId = await workersService.CreateAsync("worker", "secret");

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

        // Assert
        var result = await uowFactory
            .Create()
            .GetStatusLogRepository()
            .GetListAsync(connectorId);

        var log1 = result.First(x => x.Level == StatusLogLevel.Error);
        log1.ConnectorId.Should().Be(connectorId);
        log1.Message.Should().Be(message1);
        log1.Level.Should().Be(StatusLogLevel.Error);
        log1.TimeStamp.Should().Be(timeStamp1);

        var log2 = result.First(x => x.Level == StatusLogLevel.Info);
        log2.ConnectorId.Should().Be(connectorId);
        log2.Message.Should().Be(message2);
        log2.Level.Should().Be(StatusLogLevel.Info);
        log2.TimeStamp.Should().Be(timeStamp2);
    }

    [Fact]
    public async Task ShouldCatchException()
    {
        // Arrange
        using var unitOfWorkFactory = new SqliteUnitOfWorkFactory();

        var statusLogger = new StatusLogger(unitOfWorkFactory, new Clock(), NullLogger);

        Func<Task> func = async () => await statusLogger.AddLogAsync(Guid.NewGuid(), "Dummy message Error", StatusLogLevel.Error);

        // Act Assert
        await func.Should().NotThrowAsync();
    }
}