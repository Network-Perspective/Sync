using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Application.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services;

public class ConnectorsServiceTests
{
    private static readonly ILogger<ConnectorsService> NullLogger = NullLogger<ConnectorsService>.Instance;
    private readonly SqliteUnitOfWorkFactory _sqliteUnitOfWorkFactory = new();
    private readonly ICryptoService _cryptoService = new CryptoService();
    private readonly IClock _clock = new Clock();

    public class AddOrReplace : ConnectorsServiceTests
    {
        [Fact]
        public async Task ShouldAddNewConnector()
        {
            // Arrange
            var workersService = new WorkersService(_sqliteUnitOfWorkFactory.Create(), new Clock(), _cryptoService, NullLogger<WorkersService>.Instance);
            var workerId = await workersService.CreateAsync("worker", "secret");

            var connectorId = Guid.NewGuid();
            var networkId = Guid.NewGuid();
            var type = "Slack";
            var properties = new Dictionary<string, string>
            {
                { "StringProp", "some-prop" },
                { "BoolProp", "true" },
                { "IntProp", "321" }
            };
            var connectorService = new ConnectorsService(_sqliteUnitOfWorkFactory.Create(), new Clock(), NullLogger);

            // Act
            await connectorService.CreateAsync(connectorId, networkId, type, workerId, properties);

            // Assert
            var result = await connectorService.GetAsync(connectorId);
            result.Properties.Should().BeEquivalentTo(properties);
            result.Id.Should().Be(connectorId);
        }

        // TODO do we want that?
        //[Fact]
        //public async Task ShouldReplaceIfAlreadyExists()
        //{
        //    // Arrange
        //    var connectorId = Guid.NewGuid();
        //    var properties = new TestableNetworkProperties
        //    {
        //        StringProp = "some-prop",
        //        BoolProp = true,
        //        IntProp = 321,
        //    };

        //    var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);
        //    await connectorService.AddOrReplace(connectorId, new TestableNetworkProperties());

        //    // Act
        //    await connectorService.AddOrReplace(connectorId, properties);

        //    // Assert
        //    var result = await connectorService.GetAsync<TestableNetworkProperties>(connectorId);
        //    result.Properties.Should().BeEquivalentTo(properties);
        //    result.Id.Should().Be(connectorId);
        //}
    }

    //TODO maybe implement remove method
    //public class EnsureRemoved : ConnectorsServiceTests
    //{
    //    [Fact]
    //    public async Task ShouldRemoveExistingNetwork()
    //    {
    //        // Arrange
    //        var workersService = new WorkersService(_sqliteUnitOfWorkFactory.Create(), new Clock(), _cryptoService, NullLogger<WorkersService>.Instance);
    //        var workerId = await workersService.CreateAsync("worker", "secret");

    //        var connectorId = Guid.NewGuid();
    //        var networkId = Guid.NewGuid();
    //        var type = "Slack";
    //        var properties = new Dictionary<string, string>
    //        {
    //            { "StringProp", "some-prop" },
    //            { "BoolProp", "true" },
    //            { "IntProp", "321" }
    //        };
    //        var connectorService = new ConnectorsService(_sqliteUnitOfWorkFactory.Create(), new Clock(), NullLogger);
    //        await connectorService.CreateAsync(connectorId, networkId, type, workerId, properties);

    //        // Act
    //        await connectorService.EnsureRemovedAsync(connectorId);

    //        // Assert
    //        var result = await _unitOfWorkFactory
    //            .Create()
    //            .GetConnectorRepository<TestableNetworkProperties>()
    //            .GetAllAsync();

    //        result.Should().BeEmpty();
    //    }

    //    [Fact]
    //    public async Task ShouldNotThrowOnNonExistingNetwork()
    //    {
    //        // Arrange
    //        var connectorService = new ConnectorService(_unitOfWorkFactory, NullLogger);
    //        Func<Task> func = () => connectorService.EnsureRemovedAsync(Guid.NewGuid());

    //        // Act Assert
    //        await func.Should().NotThrowAsync();
    //    }
    //}

    public class Get : ConnectorsServiceTests
    {
        [Fact]
        public async Task ShouldThrowOnNonExisting()
        {
            // Arrange
            var connectorService = new ConnectorsService(_sqliteUnitOfWorkFactory.Create(), new Clock(), NullLogger);
            Func<Task<Connector>> func = () => connectorService.GetAsync(Guid.NewGuid());

            // Act Assert
            await func.Should().ThrowExactlyAsync<ConnectorNotFoundException>();
        }
    }

    public class ValidateExists : ConnectorsServiceTests
    {
        [Fact]
        public async Task ShouldNotThrowOnExisting()
        {
            // Arrange
            var workersService = new WorkersService(_sqliteUnitOfWorkFactory.Create(), new Clock(), _cryptoService, NullLogger<WorkersService>.Instance);
            var workerId = await workersService.CreateAsync("worker", "secret");

            var connectorId = Guid.NewGuid();
            var networkId = Guid.NewGuid();
            var type = "Slack";
            var properties = new Dictionary<string, string>
            {
                { "StringProp", "some-prop" },
                { "BoolProp", "true" },
                { "IntProp", "321" }
            };
            var connectorService = new ConnectorsService(_sqliteUnitOfWorkFactory.Create(), new Clock(), NullLogger);

            await connectorService.CreateAsync(connectorId, networkId, type, workerId, properties);

            Func<Task> func = () => connectorService.ValidateExists(connectorId);

            // Act Assert
            await func.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldThrowOnNonExisting()
        {
            // Arrange
            var connectorService = new ConnectorsService(_sqliteUnitOfWorkFactory.Create(), new Clock(), NullLogger);
            Func<Task> func = () => connectorService.ValidateExists(Guid.NewGuid());

            // Act Assert
            await func.Should().ThrowExactlyAsync<ConnectorNotFoundException>();
        }
    }
}