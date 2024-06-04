using System;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Common.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Tests
{
    public class NetworkRepositoryTests
    {
        [Fact]
        public async Task ShoudPersistNetwork()
        {
            // Arrange
            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();

            var network1 = Connector<TestableNetworkProperties>.Create(Guid.NewGuid(), new TestableNetworkProperties { StringProp = "foo", IntProp = 123, BoolProp = true }, new DateTime(1990, 10, 10));
            var network2 = Connector<TestableNetworkProperties>.Create(Guid.NewGuid(), new TestableNetworkProperties { StringProp = "bar", IntProp = -123, BoolProp = false }, new DateTime(2022, 02, 22));

            await InitializeNetworksInDatabase(unitOfWorkFactory, network1, network2);

            // Act
            var storedNetworks = await unitOfWorkFactory
                .Create()
                .GetConnectorRepository<TestableNetworkProperties>()
                .GetAllAsync();

            // Assert
            storedNetworks.Should().BeEquivalentTo(new[] { network1, network2 });
        }

        [Fact]
        public async Task ShoudRemoveNetwork()
        {
            // Arrange
            using var unitOfWorkFactory = new SqliteUnitOfWorkFactory();

            var network1 = Connector<TestableNetworkProperties>.Create(Guid.NewGuid(), new TestableNetworkProperties { StringProp = "foo", IntProp = 123, BoolProp = true }, new DateTime(1990, 10, 10));
            var network2 = Connector<TestableNetworkProperties>.Create(Guid.NewGuid(), new TestableNetworkProperties { StringProp = "bar", IntProp = -123, BoolProp = false }, new DateTime(2022, 02, 22));

            await InitializeNetworksInDatabase(unitOfWorkFactory, network1, network2);

            var unitOfWork = unitOfWorkFactory.Create();
            var repository = unitOfWork.GetConnectorRepository<TestableNetworkProperties>();

            // Act
            await repository.RemoveAsync(network1.Id);
            await unitOfWork.CommitAsync();

            var storedNetworks = await repository.GetAllAsync();

            // Assert
            storedNetworks.Should().BeEquivalentTo(new[] { network2 });
        }

        [Fact]
        public async Task ShoudFindExistingConnector()
        {
            // Arrange
            var connectorId = Guid.NewGuid();

            var unitOfWorkFactory = new SqliteUnitOfWorkFactory();

            var connector = Connector<TestableNetworkProperties>.Create(connectorId, new TestableNetworkProperties(), new DateTime(1990, 10, 10));

            await InitializeNetworksInDatabase(unitOfWorkFactory, connector);

            var unitOfWork = unitOfWorkFactory.Create();
            var repository = unitOfWork.GetConnectorRepository<TestableNetworkProperties>();

            // Act
            var existingNetwork = await repository.FindAsync(connectorId);
            var nonExistingNetwork = await repository.FindAsync(Guid.NewGuid());

            // Assert
            existingNetwork.Should().BeEquivalentTo(existingNetwork);
            nonExistingNetwork.Should().BeNull();
        }

        private static async Task InitializeNetworksInDatabase(IUnitOfWorkFactory unitOfWorkFactory, params Connector<TestableNetworkProperties>[] connectors)
        {
            var unitOfWork = unitOfWorkFactory.Create();
            var connectorRepository = unitOfWork.GetConnectorRepository<TestableNetworkProperties>();

            foreach (var connector in connectors)
                await connectorRepository.AddAsync(connector);

            await unitOfWork.CommitAsync();
        }
    }
}