using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class NetworkServiceTests
    {
        private static readonly ILogger<NetworkService> NullLogger = NullLogger<NetworkService>.Instance;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory = new InMemoryUnitOfWorkFactory();
        private readonly Mock<ISyncScheduler> _syncSchedulerMock = new Mock<ISyncScheduler>();

        public NetworkServiceTests()
        {
            _syncSchedulerMock.Reset();
        }

        public class AddOrReplace : NetworkServiceTests
        {
            [Fact]
            public async Task ShouldAddNewNetwork()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var properties = new TestableNetworkProperties
                {
                    StringProp = "some-prop",
                    BoolProp = true,
                    IntProp = 321,
                };
                var networkService = new NetworkService(_unitOfWorkFactory, NullLogger);

                // Act
                await networkService.AddOrReplace(networkId, properties);

                // Assert
                var result = await networkService.GetAsync<TestableNetworkProperties>(networkId);
                result.Properties.Should().BeEquivalentTo(properties);
                result.NetworkId.Should().Be(networkId);
            }

            [Fact]
            public async Task ShouldReplaceIfAlreadyExists()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var properties = new TestableNetworkProperties
                {
                    StringProp = "some-prop",
                    BoolProp = true,
                    IntProp = 321,
                };

                var networkService = new NetworkService(_unitOfWorkFactory, NullLogger);
                await networkService.AddOrReplace(networkId, new TestableNetworkProperties());

                // Act
                await networkService.AddOrReplace(networkId, properties);

                // Assert
                var result = await networkService.GetAsync<TestableNetworkProperties>(networkId);
                result.Properties.Should().BeEquivalentTo(properties);
                result.NetworkId.Should().Be(networkId);
            }
        }

        public class EnsureRemoved : NetworkServiceTests
        {
            [Fact]
            public async Task ShouldRemoveExistingNetwork()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var networkService = new NetworkService(_unitOfWorkFactory, NullLogger);
                await networkService.AddOrReplace(networkId, new TestableNetworkProperties());

                // Act
                await networkService.EnsureRemovedAsync(networkId);

                // Assert
                var result = await _unitOfWorkFactory
                    .Create()
                    .GetNetworkRepository<TestableNetworkProperties>()
                    .GetAllAsync();

                result.Should().BeEmpty();
            }

            [Fact]
            public async Task ShouldNotThrowOnNonExistingNetwork()
            {
                // Arrange
                var networkService = new NetworkService(_unitOfWorkFactory, NullLogger);
                Func<Task> func = () => networkService.EnsureRemovedAsync(Guid.NewGuid());

                // Act Assert
                await func.Should().NotThrowAsync();
            }
        }

        public class Get : NetworkServiceTests
        {
            [Fact]
            public async Task ShouldThrowOnNonExisting()
            {
                // Arrange
                var networkService = new NetworkService(_unitOfWorkFactory, NullLogger);
                Func<Task<Network<TestableNetworkProperties>>> func = () => networkService.GetAsync<TestableNetworkProperties>(Guid.NewGuid());

                // Act Assert
                await func.Should().ThrowExactlyAsync<NetworkNotFoundException>();
            }
        }

        public class ValidateExists : NetworkServiceTests
        {
            [Fact]
            public async Task ShouldNotThrowOnExisting()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var networkService = new NetworkService(_unitOfWorkFactory, NullLogger);
                await networkService.AddOrReplace(networkId, new TestableNetworkProperties());

                Func<Task> func = () => networkService.ValidateExists(networkId);

                // Act Assert
                await func.Should().NotThrowAsync();
            }

            [Fact]
            public async Task ShouldThrowOnNonExisting()
            {
                // Arrange
                var networkService = new NetworkService(_unitOfWorkFactory, NullLogger);
                Func<Task> func = () => networkService.ValidateExists(Guid.NewGuid());

                // Act Assert
                await func.Should().ThrowExactlyAsync<NetworkNotFoundException>();
            }
        }
    }
}