using System.Threading.Tasks;
using System;

using NetworkPerspective.Sync.Office365.Tests.Fixtures;
using NetworkPerspective.Sync.Orchestrator.Client;

using Xunit;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using NetworkPerspective.Sync.Orchestrator.Tests.Fixtures;

namespace NetworkPerspective.Sync.Orchestrator.Tests;

[Collection(TestsCollection.Name)]
public class ConnectorsControllerTests
{
    private readonly OrchestratorServiceFixture _service;

    public ConnectorsControllerTests(OrchestratorServiceFixture service)
    {
        _service = service;
    }

    [Fact]
    public async Task ShouldReturnCreatedConnectors()
    {
        // Arrange
        var httpClient = _service.CreateDefaultClient();

        var wokrersClient = new WorkersClient(httpClient);
        var connectorsClient = new ConnectorsClient(httpClient);
        await wokrersClient.WorkersPostAsync(new CreateWorkerDto { Name = "worker", Secret = "secret1" });

        var workers = await wokrersClient.WorkersGetAsync();
        var workerId = workers.Single().Id;

        // Act


        await connectorsClient.ConnectorsPostAsync(new CreateConnectorDto 
        { 
            WorkerId = workerId,
            Type = "Google",
            Properties = new ConnectorPropertyDto[]
            {
                new() { Key = "key1", Value = "value1" },
                new() { Key = "key2", Value = "value2" }
            }
        });

        // Assert
        var actual = await connectorsClient.ConnectorsGetAsync(workerId);
        actual.Should().HaveCount(1);
        actual.Single().Type.Should().Be("Google");
        actual.Single().WorkerId.Should().Be(workerId);
    }
}
