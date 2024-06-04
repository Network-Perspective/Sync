using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class StatusLoggerTests
    {
        private static readonly ILogger<StatusLogger> NullLogger = NullLogger<StatusLogger>.Instance;

        [Fact]
        public async Task ShouldReturnPersistedLogs()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            const string message1 = "Dummy message Error";
            const string message2 = "Dummy message Info";
            var timeStamp1 = DateTime.UtcNow;
            var timeStamp2 = DateTime.UtcNow;

            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();
            var clockMock = new Mock<IClock>();
            clockMock
                .SetupSequence(x => x.UtcNow())
                .Returns(timeStamp1)
                .Returns(timeStamp2);

            var unitOfWork = unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetConnectorRepository<TestableNetworkProperties>();
            await networkRepository.AddAsync(Connector<TestableNetworkProperties>.Create(connectorId, new TestableNetworkProperties(), DateTime.UtcNow));
            await unitOfWork.CommitAsync();

            var statusLogger = new StatusLogger(connectorId, unitOfWorkFactory, clockMock.Object, NullLogger);

            // Act
            await statusLogger.AddLogAsync(message1, StatusLogLevel.Error);
            await statusLogger.AddLogAsync(message2, StatusLogLevel.Info);

            // Assert
            var result = await unitOfWork
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

            var statusLogger = new StatusLogger(Guid.NewGuid(), unitOfWorkFactory, new Clock(), NullLogger);

            Func<Task> func = async () => await statusLogger.AddLogAsync("Dummy message Error", StatusLogLevel.Error);

            // Act Assert
            await func.Should().NotThrowAsync();
        }
    }
}