using System;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Networks;
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

            var network1 = Network<TestableNetworkProperties>.Create(Guid.NewGuid(), new TestableNetworkProperties { StringProp = "foo", IntProp = 123, BoolProp = true }, new DateTime(1990, 10, 10));
            var network2 = Network<TestableNetworkProperties>.Create(Guid.NewGuid(), new TestableNetworkProperties { StringProp = "bar", IntProp = -123, BoolProp = false }, new DateTime(2022, 02, 22));

            await InitializeNetworksInDatabase(unitOfWorkFactory, network1, network2);

            // Act
            var storedNetworks = await unitOfWorkFactory
                .Create()
                .GetNetworkRepository<TestableNetworkProperties>()
                .GetAllAsync();

            // Assert
            storedNetworks.Should().BeEquivalentTo(new[] { network1, network2 });
        }

        [Fact]
        public async Task ShoudRemoveNetwork()
        {
            // Arrange
            using var unitOfWorkFactory = new SqliteUnitOfWorkFactory();

            var network1 = Network<TestableNetworkProperties>.Create(Guid.NewGuid(), new TestableNetworkProperties { StringProp = "foo", IntProp = 123, BoolProp = true }, new DateTime(1990, 10, 10));
            var network2 = Network<TestableNetworkProperties>.Create(Guid.NewGuid(), new TestableNetworkProperties { StringProp = "bar", IntProp = -123, BoolProp = false }, new DateTime(2022, 02, 22));

            await InitializeNetworksInDatabase(unitOfWorkFactory, network1, network2);

            var unitOfWork = unitOfWorkFactory.Create();
            var repository = unitOfWork.GetNetworkRepository<TestableNetworkProperties>();

            // Act
            await repository.RemoveAsync(network1.NetworkId);
            await unitOfWork.CommitAsync();

            var storedNetworks = await repository.GetAllAsync();

            // Assert
            storedNetworks.Should().BeEquivalentTo(new[] { network2 });
        }

        [Fact]
        public async Task ShoudFindExistingNetwork()
        {
            // Arrange
            var networkId = Guid.NewGuid();

            var unitOfWorkFactory = new SqliteUnitOfWorkFactory();

            var network = Network<TestableNetworkProperties>.Create(networkId, new TestableNetworkProperties(), new DateTime(1990, 10, 10));

            await InitializeNetworksInDatabase(unitOfWorkFactory, network);

            var unitOfWork = unitOfWorkFactory.Create();
            var repository = unitOfWork.GetNetworkRepository<TestableNetworkProperties>();

            // Act
            var existingNetwork = await repository.FindAsync(networkId);
            var nonExistingNetwork = await repository.FindAsync(Guid.NewGuid());

            // Assert
            existingNetwork.Should().BeEquivalentTo(existingNetwork);
            nonExistingNetwork.Should().BeNull();
        }

        private static async Task InitializeNetworksInDatabase(IUnitOfWorkFactory unitOfWorkFactory, params Network<TestableNetworkProperties>[] networks)
        {
            var unitOfWork = unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetNetworkRepository<TestableNetworkProperties>();

            foreach (var network in networks)
                await networkRepository.AddAsync(network);

            await unitOfWork.CommitAsync();
        }
    }
}