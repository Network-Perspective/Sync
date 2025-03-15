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

public class WorkerRouterTests
{
    private const string WorkerName = "worker-name";
    private const string ConnectionId = "connection-id";

    private readonly ILogger<WorkerRouter> _logger = NullLogger<WorkerRouter>.Instance;

    private readonly Mock<IConnectionsLookupTable> _connectionsLookupTableMock = new();
    private readonly Mock<IHubClients<IWorkerClient>> _mockClients = new();
    private readonly Mock<IWorkerClient> _mockClientProxy = new();
    private readonly Mock<IHubContext<WorkerHubV1, IWorkerClient>> _mockHubContext = new();

    public WorkerRouterTests()
    {
        _connectionsLookupTableMock.Reset();
        _mockClients.Reset();
        _mockClientProxy.Reset();
        _mockHubContext.Reset();

        HubV1MapsterConfig.RegisterMappings(TypeAdapterConfig.GlobalSettings);

        var workerConnection = new WorkerConnection(WorkerName, ConnectionId);

        _connectionsLookupTableMock
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(workerConnection);

        _mockHubContext
            .Setup(x => x.Clients)
            .Returns(_mockClients.Object);

        _mockClients
            .Setup(x => x.All)
            .Returns(_mockClientProxy.Object);

        _mockClients
            .Setup(x => x.Client(ConnectionId))
            .Returns(_mockClientProxy.Object);
    }

    public class GetConnectorStatus : WorkerRouterTests
    {
        [Fact]
        public async Task ShouldReturnIdleStatus()
        {
            // Arrange
            var isAuthorized = false;

            var hub = new WorkerRouter(_mockHubContext.Object, _connectionsLookupTableMock.Object, _logger);

            _mockClientProxy
                .Setup(x => x.GetConnectorStatusAsync(It.IsAny<ConnectorStatusRequest>()))
                .ReturnsAsync(new ConnectorStatusResponse()
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

            var hub = new WorkerRouter(_mockHubContext.Object, _connectionsLookupTableMock.Object, _logger);

            _mockClientProxy
                .Setup(x => x.GetConnectorStatusAsync(It.IsAny<ConnectorStatusRequest>()))
                .ReturnsAsync(new ConnectorStatusResponse()
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