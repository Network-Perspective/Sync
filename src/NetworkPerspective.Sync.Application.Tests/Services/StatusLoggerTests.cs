using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Networks;
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
            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();

            var networkId = Guid.NewGuid();

            var unitOfWork = unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetNetworkRepository<TestableNetworkProperties>();
            await networkRepository.AddAsync(Network<TestableNetworkProperties>.Create(networkId, new TestableNetworkProperties(), DateTime.UtcNow));
            await unitOfWork.CommitAsync();

            var status1 = StatusLog.Create(networkId, "Dummy message Error", StatusLogLevel.Error, DateTime.UtcNow);
            var status2 = StatusLog.Create(networkId, "Dummy message Info", StatusLogLevel.Info, DateTime.UtcNow);

            var statusLogger = new StatusLogger(unitOfWorkFactory, NullLogger);

            // Act
            await statusLogger.AddLogAsync(status1);
            await statusLogger.AddLogAsync(status2);

            // Assert
            var result = await unitOfWork
                .GetStatusLogRepository()
                .GetListAsync(networkId);
            result.Should().BeEquivalentTo(new[] { status1, status2 });
        }

        [Fact]
        public async Task ShouldCatchException()
        {
            // Arrange
            using var unitOfWorkFactory = new SqliteUnitOfWorkFactory();

            var status1 = StatusLog.Create(Guid.NewGuid(), "Dummy message Error", StatusLogLevel.Error, DateTime.UtcNow);

            var statusLogger = new StatusLogger(unitOfWorkFactory, NullLogger);

            Func<Task> func = async () => await statusLogger.AddLogAsync(status1);

            // Act Assert
            await func.Should().NotThrowAsync();
        }
    }
}