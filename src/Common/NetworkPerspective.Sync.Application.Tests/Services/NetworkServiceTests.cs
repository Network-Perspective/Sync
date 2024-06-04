using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class NetworkServiceTests
    {
        private static readonly ILogger<ConnectorService> NullLogger = NullLogger<ConnectorService>.Instance;
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
                var connectorId = Guid.NewGuid();
                var properties = new TestableNetworkProperties
                {
                    StringProp = "some-prop",
                    BoolProp = true,
                    IntProp = 321,
                };
                var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);

                // Act
                await connectorService.AddOrReplace(connectorId, properties);

                // Assert
                var result = await connectorService.GetAsync<TestableNetworkProperties>(connectorId);
                result.Properties.Should().BeEquivalentTo(properties);
                result.Id.Should().Be(connectorId);
            }

            [Fact]
            public async Task ShouldReplaceIfAlreadyExists()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var properties = new TestableNetworkProperties
                {
                    StringProp = "some-prop",
                    BoolProp = true,
                    IntProp = 321,
                };

                var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);
                await connectorService.AddOrReplace(connectorId, new TestableNetworkProperties());

                // Act
                await connectorService.AddOrReplace(connectorId, properties);

                // Assert
                var result = await connectorService.GetAsync<TestableNetworkProperties>(connectorId);
                result.Properties.Should().BeEquivalentTo(properties);
                result.Id.Should().Be(connectorId);
            }
        }

        public class EnsureRemoved : NetworkServiceTests
        {
            [Fact]
            public async Task ShouldRemoveExistingNetwork()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);
                await connectorService.AddOrReplace(connectorId, new TestableNetworkProperties());

                // Act
                await connectorService.EnsureRemovedAsync(connectorId);

                // Assert
                var result = await _unitOfWorkFactory
                    .Create()
                    .GetConnectorRepository<TestableNetworkProperties>()
                    .GetAllAsync();

                result.Should().BeEmpty();
            }

            [Fact]
            public async Task ShouldNotThrowOnNonExistingNetwork()
            {
                // Arrange
                var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);
                Func<Task> func = () => connectorService.EnsureRemovedAsync(Guid.NewGuid());

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
                var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);
                Func<Task<Connector<TestableNetworkProperties>>> func = () => connectorService.GetAsync<TestableNetworkProperties>(Guid.NewGuid());

                // Act Assert
                await func.Should().ThrowExactlyAsync<ConnectorNotFoundException>();
            }
        }

        public class ValidateExists : NetworkServiceTests
        {
            [Fact]
            public async Task ShouldNotThrowOnExisting()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);
                await connectorService.AddOrReplace(connectorId, new TestableNetworkProperties());

                Func<Task> func = () => connectorService.ValidateExists(connectorId);

                // Act Assert
                await func.Should().NotThrowAsync();
            }

            [Fact]
            public async Task ShouldThrowOnNonExisting()
            {
                // Arrange
                var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);
                Func<Task> func = () => connectorService.ValidateExists(Guid.NewGuid());

                // Act Assert
                await func.Should().ThrowExactlyAsync<ConnectorNotFoundException>();
            }
        }
    }
}