using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Office365.Tests.Fixtures;
using NetworkPerspective.Sync.Orchestrator.Client;
using NetworkPerspective.Sync.Orchestrator.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.ComponentTests;

[Collection(TestsCollection.Name)]
public class WorkersHubTests
{
    private readonly OrchestratorServiceFixture _service;

    public WorkersHubTests(OrchestratorServiceFixture service)
    {
        _service = service;
    }

    [Fact]
    public async Task ShouldConnectWithValidCredentials()
    {
        // Arrange
        var workerName = "client_1";
        var workerSecret = "pass1";
        var workersClient = new WorkersClient(_service.CreateDefaultClient());

        await workersClient.WorkersPostAsync(new CreateWorkerDto
        {
            Name = workerName,
            Secret = workerSecret,
        });
        var worker = await workersClient.WorkersGetAsync(workerName);
        await workersClient.AuthAsync(worker.Id);

        var config = Options.Create(new WorkerHubClientConfig
        {
            BaseUrl = _service.Server.BaseAddress.ToString()
        });
        var hubClient = new WorkerHubClient(config, NullLogger<IWorkerHubClient>.Instance);

        // Act
        await hubClient.ConnectAsync(connectionConfiguration: x => x.WithUrl($"{_service.Server.BaseAddress}ws/v1/workers-hub", options =>
        {
            options.AccessTokenProvider = () =>
            {
                var bytes = Encoding.UTF8.GetBytes($"{workerName}:{workerSecret}");
                return Task.FromResult(Convert.ToBase64String(bytes));
            };
            options.HttpMessageHandlerFactory = _ => _service.Server.CreateHandler();
        }));

        // Assert
        var correlationId = Guid.NewGuid();
        var pongResponse = await hubClient.PingAsync(new PingDto { CorrelationId = correlationId, Timestamp = DateTime.UtcNow });
        pongResponse.CorrelationId.Should().Be(correlationId);
        await workersClient.WorkersDeleteAsync(worker.Id);
    }

    [Fact]
    public async Task ShouldNotConnectWithInvalidCredentials()
    {
        // Arrange
        var workerName = "client_1";
        var workerSecret = "invalid-pass";
        var workersClient = new WorkersClient(_service.CreateDefaultClient());

        await workersClient.WorkersPostAsync(new CreateWorkerDto
        {
            Name = workerName,
            Secret = workerSecret,
        });
        var worker = await workersClient.WorkersGetAsync(workerName);
        await workersClient.AuthAsync(worker.Id);

        var config = Options.Create(new WorkerHubClientConfig
        {
            BaseUrl = _service.Server.BaseAddress.ToString()
        });
        var hubClient = new WorkerHubClient(config, NullLogger<IWorkerHubClient>.Instance);

        // Act
        Func<Task> func = () => hubClient.ConnectAsync(connectionConfiguration: x => x.WithUrl($"{_service.Server.BaseAddress}ws/v1/workers-hub", options =>
        {
            options.HttpMessageHandlerFactory = _ => _service.Server.CreateHandler();
        }));

        // Assert
        await func.Should()
            .ThrowAsync<HttpRequestException>()
            .Where(x => x.StatusCode == HttpStatusCode.Unauthorized);
        await workersClient.WorkersDeleteAsync(worker.Id);
    }

    [Fact]
    public async Task ShouldNotConnectorNotAuthed()
    {
        // Arrange
        var workerName = "client_1";
        var workerSecret = "pass1";
        var workersClient = new WorkersClient(_service.CreateDefaultClient());

        await workersClient.WorkersPostAsync(new CreateWorkerDto
        {
            Name = workerName,
            Secret = workerSecret,
        });
        var worker = await workersClient.WorkersGetAsync(workerName);

        var config = Options.Create(new WorkerHubClientConfig
        {
            BaseUrl = _service.Server.BaseAddress.ToString()
        });
        var hubClient = new WorkerHubClient(config, NullLogger<IWorkerHubClient>.Instance);

        // Act
        Func<Task> func = () => hubClient.ConnectAsync(connectionConfiguration: x => x.WithUrl($"{_service.Server.BaseAddress}ws/v1/workers-hub", options =>
        {
            options.AccessTokenProvider = () =>
            {
                var bytes = Encoding.UTF8.GetBytes($"{workerName}:{workerSecret}");
                return Task.FromResult(Convert.ToBase64String(bytes));
            };
            options.HttpMessageHandlerFactory = _ => _service.Server.CreateHandler();
        }));

        // Assert
        await func.Should()
            .ThrowAsync<HttpRequestException>()
            .Where(x => x.StatusCode == HttpStatusCode.Unauthorized);
        await workersClient.WorkersDeleteAsync(worker.Id);
    }
}