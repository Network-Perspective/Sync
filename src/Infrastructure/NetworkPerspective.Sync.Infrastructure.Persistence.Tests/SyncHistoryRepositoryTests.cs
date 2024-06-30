using System;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Utils.Models;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Tests
{
    public class SyncHistoryRepositoryTests
    {
        [Fact]
        public async Task ShoudPersistLog()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();
            var unitOfWork = unitOfWorkFactory.Create();

            var networkRepository = unitOfWork.GetConnectorRepository<TestableNetworkProperties>();
            await networkRepository.AddAsync(Connector<TestableNetworkProperties>.Create(connectorId, new TestableNetworkProperties(), DateTime.UtcNow));

            var syncHistoryRepository = unitOfWork.GetSyncHistoryRepository();
            var syncHistoryEntry1 = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 2)));
            var syncHistoryEntry2 = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(new DateTime(2021, 2, 2), new DateTime(2021, 2, 3)));
            var syncHistoryEntry3 = SyncHistoryEntry.Create(Guid.Empty, DateTime.UtcNow, new TimeRange(new DateTime(2021, 2, 3), new DateTime(2021, 2, 4)));

            // Act
            await syncHistoryRepository.AddAsync(syncHistoryEntry1);
            await syncHistoryRepository.AddAsync(syncHistoryEntry2);
            await syncHistoryRepository.AddAsync(syncHistoryEntry3);
            await unitOfWork.CommitAsync();
            var lastSyncHistoryEntry = await syncHistoryRepository.FindLastLogAsync(connectorId);

            // Assert
            lastSyncHistoryEntry.Should().BeEquivalentTo(syncHistoryEntry2);
        }

        [Fact]
        public async Task ShouldThrowOnNotExitingNetwork()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            using var unitOfWorkFactory = new SqliteUnitOfWorkFactory();
            var unitOfWork = unitOfWorkFactory.Create();

            var repository = unitOfWork.GetSyncHistoryRepository();

            var syncHistoryEntry = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 2)));

            // Act
            await repository.AddAsync(syncHistoryEntry);

            // Assert
            Func<Task> func = () => unitOfWork.CommitAsync();
            await func.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task ShoudBeAbleToRemoveAll()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();
            var unitOfWork = unitOfWorkFactory.Create();

            var networkRepository = unitOfWork.GetConnectorRepository<TestableNetworkProperties>();
            await networkRepository.AddAsync(Connector<TestableNetworkProperties>.Create(connectorId, new TestableNetworkProperties(), DateTime.UtcNow));

            var syncHistoryRepository = unitOfWork.GetSyncHistoryRepository();
            var syncHistoryEntry1 = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 2)));
            var syncHistoryEntry2 = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(new DateTime(2021, 2, 2), new DateTime(2021, 2, 3)));

            // Act
            await syncHistoryRepository.AddAsync(syncHistoryEntry1);
            await syncHistoryRepository.AddAsync(syncHistoryEntry2);
            await unitOfWork.CommitAsync();

            await syncHistoryRepository.RemoveAllAsync(connectorId);
            await unitOfWork.CommitAsync();

            // Assert
            var lastSyncHistoryEntry = await syncHistoryRepository.FindLastLogAsync(connectorId);
            lastSyncHistoryEntry.Should().BeNull();
        }

        [Fact]
        public async Task ShoudRemoveAllOnlyForGivenNetwork()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();
            var unitOfWork = unitOfWorkFactory.Create();

            var networkRepository = unitOfWork.GetConnectorRepository<TestableNetworkProperties>();
            await networkRepository.AddAsync(Connector<TestableNetworkProperties>.Create(connectorId, new TestableNetworkProperties(), DateTime.UtcNow));

            var syncHistoryRepository = unitOfWork.GetSyncHistoryRepository();
            var syncHistoryEntry1 = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 2)));
            var syncHistoryEntry2 = SyncHistoryEntry.Create(connectorId, DateTime.UtcNow, new TimeRange(new DateTime(2021, 2, 2), new DateTime(2021, 2, 3)));
            var syncHistoryEntry3 = SyncHistoryEntry.Create(Guid.Empty, DateTime.UtcNow, new TimeRange(new DateTime(2021, 2, 3), new DateTime(2021, 2, 4)));

            // Act
            await syncHistoryRepository.AddAsync(syncHistoryEntry1);
            await syncHistoryRepository.AddAsync(syncHistoryEntry2);
            await syncHistoryRepository.AddAsync(syncHistoryEntry3);
            await unitOfWork.CommitAsync();

            await syncHistoryRepository.RemoveAllAsync(Guid.NewGuid());
            await unitOfWork.CommitAsync();

            // Assert
            var lastSyncHistoryEntry = await syncHistoryRepository.FindLastLogAsync(connectorId);
            lastSyncHistoryEntry.Should().BeEquivalentTo(syncHistoryEntry2);
        }
    }
}