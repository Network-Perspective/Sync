using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Mapster;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Contract.V1;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Hubs.V1;
using NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.Hubs.V1;

public class WorkerHubV1Tests
{
    private const string WorkerName = "worker-name";
    private const string ConnectionId = "connection-id";

    private readonly Mock<IConnectionsLookupTable> _connectionsLookupTableMock = new();
    private readonly Mock<IStatusLogger> _statusLoggerMock = new();
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<IClock> _clockMock = new();
    private readonly ILogger<WorkerHubV1> _logger = NullLogger<WorkerHubV1>.Instance;

    private readonly Mock<IHubCallerClients<IWorkerClient>> _clientsMock = new();
    private readonly Mock<IWorkerClient> _clientProxyMock = new();

    public WorkerHubV1Tests()
    {
        HubV1MapsterConfig.RegisterMappings(TypeAdapterConfig.GlobalSettings);

        _connectionsLookupTableMock.Reset();
        _statusLoggerMock.Reset();
        _serviceProviderMock.Reset();
        _clockMock.Reset();

        var workerConnection = new WorkerConnection(WorkerName, ConnectionId);

        _connectionsLookupTableMock
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(workerConnection);

        _clientsMock
            .Setup(x => x.Client(ConnectionId))
            .Returns(_clientProxyMock.Object);
    }


    public class GetConnectorStatus : WorkerHubV1Tests
    {
        [Fact]
        public async Task ShouldReturnIdleStatus()
        {
            // Arrange
            var isAuthorized = false;

            var hub = new WorkerHubV1(_connectionsLookupTableMock.Object, _statusLoggerMock.Object, _serviceProviderMock.Object, _clockMock.Object, _logger)
            {
                Clients = _clientsMock.Object
            };

            _clientProxyMock
                .Setup(x => x.GetConnectorStatusAsync(It.IsAny<GetConnectorStatusDto>()))
                .ReturnsAsync(new ConnectorStatusDto()
                {
                    IsRunning = false,
                    IsAuthorized = isAuthorized,
                });

            // Act
            var actualStatus = await hub.GetConnectorStatusAsync(WorkerName, Guid.NewGuid(), Guid.NewGuid(), new Dictionary<string, string>(), "Slack");

            // Assert
            var expectedStatus = ConnectorStatus.Idle(isAuthorized);
            actualStatus.Should().BeEquivalentTo(expectedStatus);
        }

        [Fact]
        public async Task ShouldReturnRunningStatus()
        {
            // Arrange
            var isAuthorized = true;
            var caption = "caption";
            var description = "description";
            var completionRate = 42.42;

            var hub = new WorkerHubV1(_connectionsLookupTableMock.Object, _statusLoggerMock.Object, _serviceProviderMock.Object, _clockMock.Object, _logger)
            {
                Clients = _clientsMock.Object
            };

            _clientProxyMock
                .Setup(x => x.GetConnectorStatusAsync(It.IsAny<GetConnectorStatusDto>()))
                .ReturnsAsync(new ConnectorStatusDto()
                {
                    IsRunning = true,
                    IsAuthorized = isAuthorized,
                    CurrentTaskCaption = caption,
                    CurrentTaskDescription = description,
                    CurrentTaskCompletionRate = completionRate
                });

            // Act
            var actualStatus = await hub.GetConnectorStatusAsync(WorkerName, Guid.NewGuid(), Guid.NewGuid(), new Dictionary<string, string>(), "Slack");

            // Assert
            var expectedTaskStatus = ConnectorTaskStatus.Create(caption, description, completionRate);
            var expectedStatus = ConnectorStatus.Running(isAuthorized, expectedTaskStatus);
            actualStatus.Should().BeEquivalentTo(expectedStatus);
        }
    }
}