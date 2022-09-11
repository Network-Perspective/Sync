using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class SyncHistoryServiceTests
    {
        private static readonly ILogger<SyncHistoryService> NullLogger = NullLogger<SyncHistoryService>.Instance;

        [Fact]
        public async Task ShouldReturnEndOfLastSyncPeriod()
        {
            // Arrange
            var syncStart = DateTime.UtcNow;
            var syncEnd = DateTime.UtcNow.AddDays(1);
            var networkId = Guid.NewGuid();

            var record = new SyncHistoryEntry(networkId, DateTime.UtcNow, new TimeRange(syncStart, syncEnd));

            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();
            using var unitOfWork = unitOfWorkFactory.Create();

            var networkRepository = unitOfWork.GetNetworkRepository<TestableNetworkProperties>();
            await networkRepository.AddAsync(Network<TestableNetworkProperties>.Create(networkId, new TestableNetworkProperties(), DateTime.UtcNow));
            await unitOfWork.CommitAsync();

            var service = new SyncHistoryService(unitOfWork, new Clock(), CreateOptions(1), NullLogger);
            await service.SaveLogAsync(record);

            // Act
            var result = await service.EvaluateSyncStartAsync(networkId);

            // Aseert
            result.Should().Be(syncEnd);
        }

        [Fact]
        public async Task ShouldReturnDefaultDaysBack()
        {
            // Arrange
            const int configLookbackInDays = 10;
            var now = DateTime.UtcNow;

            var clockMock = new Mock<IClock>();
            clockMock
                .Setup(x => x.UtcNow())
                .Returns(now);

            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();
            using var unitOfWork = unitOfWorkFactory.Create();

            var service = new SyncHistoryService(unitOfWork, clockMock.Object, CreateOptions(configLookbackInDays), NullLogger);

            // Act
            var result = await service.EvaluateSyncStartAsync(Guid.NewGuid());

            // Aseert
            result.Should().Be(now.AddDays(-configLookbackInDays));
        }

        private IOptions<SyncConfig> CreateOptions(int defaultSyncLookbackInDays)
            => Options.Create(new SyncConfig { DefaultSyncLookbackInDays = defaultSyncLookbackInDays });
    }
}