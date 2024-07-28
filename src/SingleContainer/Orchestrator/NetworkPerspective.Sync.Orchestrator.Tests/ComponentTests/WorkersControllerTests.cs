using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Office365.Tests.Fixtures;
using NetworkPerspective.Sync.Orchestrator.Client;
using NetworkPerspective.Sync.Orchestrator.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.ComponentTests;

[Collection(TestsCollection.Name)]
public class WorkersControllerTests
{
    private readonly OrchestratorServiceFixture _service;

    public WorkersControllerTests(OrchestratorServiceFixture service)
    {
        _service = service;
    }

    [Fact]
    public async Task ShouldReturnCreatedWorkers()
    {
        // Arrange
        var httpClient = _service.CreateDefaultClient();

        var client = new WorkersClient(httpClient);

        // Act
        await client.WorkersPostAsync(new CreateWorkerDto { Name = "worker1", Secret = "secret1" });
        await client.WorkersPostAsync(new CreateWorkerDto { Name = "worker2", Secret = "secret2" });
        await client.WorkersPostAsync(new CreateWorkerDto { Name = "worker3", Secret = "secret3" });

        var current = await client.WorkersGetAsync();

        var tobeAuthorized = current.Single(x => x.Name == "worker1").Id;
        await client.AuthAsync(tobeAuthorized);

        var toBeDeletedId = current.Single(x => x.Name == "worker2").Id;
        await client.WorkersDeleteAsync(toBeDeletedId);

        var unauthorized = current.Single(x => x.Name == "worker3").Id;

        // Assert
        var actual = await client.WorkersGetAsync();

        actual.Should().ContainEquivalentOf(new WorkerDto { Id = tobeAuthorized, Name = "worker1", IsAuthorized = true });
        actual.Should().NotContainEquivalentOf(new WorkerDto { Id = toBeDeletedId, Name = "worker2", IsAuthorized = false });
        actual.Should().ContainEquivalentOf(new WorkerDto { Id = unauthorized, Name = "worker3", IsAuthorized = false });
    }

    [Fact]
    public async Task ShouldReturn401OnInvalidApiKey()
    {
        // Arrange
        var httpClient = _service.CreateDefaultClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

        var client = new WorkersClient(httpClient);

        // Act
        Func<Task> func = () => client.WorkersPostAsync(new CreateWorkerDto { Name = "worker1", Secret = "secret1" });

        (await func.Should().ThrowExactlyAsync<OrchestratorClientException>()).And.StatusCode.Should().Be(401);
    }
}