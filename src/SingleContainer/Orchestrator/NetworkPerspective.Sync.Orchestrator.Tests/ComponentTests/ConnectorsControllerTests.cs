using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Office365.Tests.Fixtures;
using NetworkPerspective.Sync.Orchestrator.Client;
using NetworkPerspective.Sync.Orchestrator.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.ComponentTests;

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
        var workerName = Guid.NewGuid().ToString();
        var httpClient = _service.CreateDefaultClient();

        var wokrersClient = new WorkersClient(httpClient);
        var connectorsClient = new ConnectorsClient(httpClient);
        await wokrersClient.WorkersPostAsync(new CreateWorkerDto { Name = workerName, Secret = "secret1" });

        var workers = await wokrersClient.WorkersGetAsync();
        var workerId = workers.Single(x => x.Name == workerName).Id;

        // Act
        await connectorsClient.ConnectorsPostAsync(new CreateConnectorDto
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            Type = "Google",
            Properties =
            [
                new() { Key = "key1", Value = "value1" },
                new() { Key = "key2", Value = "value2" }
            ]
        });

        // Assert
        var actual = await connectorsClient.ConnectorsGetAsync(workerId);
        actual.Should().HaveCount(1);
        actual.Single().Type.Should().Be("Google");
        actual.Single().WorkerId.Should().Be(workerId);
    }
}