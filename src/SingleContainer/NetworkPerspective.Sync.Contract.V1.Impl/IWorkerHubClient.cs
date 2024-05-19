using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1.Impl;

public interface IWorkerHubClient : IOrchestratorClient
{
    Task ConnectAsync(Action<IHubConnectionBuilder> configureConnection = null, CancellationToken stoppingToken = default);
}

internal class WorkerHubClient : IWorkerHubClient
{
    private HubConnection _connection;
    private readonly WorkerHubClientConfig _config;
    private readonly ILogger<IWorkerHubClient> _logger;

    public WorkerHubClient(IOptions<WorkerHubClientConfig> config, ILogger<IWorkerHubClient> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    private static Task<string> TokenFactory()
    {
        var name = "client_1";
        var pass = "pass1";
        var tokenBytes = Encoding.UTF8.GetBytes($"{name}:{pass}");
        var tokenBase64 = Convert.ToBase64String(tokenBytes);
        return Task.FromResult(tokenBase64);
    }

    public async Task ConnectAsync(Action<IHubConnectionBuilder> configureConnection = null, CancellationToken stoppingToken = default)
    {
        var hubUrl = $"{_config.BaseUrl}ws/v1/workers-hub";

        var builder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = TokenFactory;
            })
            .WithAutomaticReconnect();

        configureConnection?.Invoke(builder);

        _connection = builder.Build();
        InitializeConnection();

        await _connection.StartAsync(stoppingToken);
    }

    public Task<AckDto> SyncCompletedAsync(SyncCompletedDto syncCompleted)
    {
        return _connection.InvokeAsync<AckDto>(nameof(IOrchestratorClient.SyncCompletedAsync), syncCompleted);
    }

    public async Task<PongDto> PingAsync(PingDto ping)
    {
        var result = await _connection.InvokeAsync<PongDto>(nameof(IOrchestratorClient.PingAsync), ping);
        var timespan = DateTime.UtcNow - result.PingTimestamp;
        _logger.LogInformation("Ping response took {timespan}ms", Math.Round(timespan.TotalMilliseconds));
        return result;
    }

    private void InitializeConnection()
    {
        _connection.On<StartSyncDto, AckDto>(nameof(IWorkerClient.StartSyncAsync), async x =>
        {
            _logger.LogInformation("Received request to start sync '{correlationId}'", x.CorrelationId);

            await Task.Yield();

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                await SyncCompletedAsync(new SyncCompletedDto { CorrelationId = Guid.NewGuid() });
            });

            _logger.LogInformation("Sending ack '{correlationId}'", x.CorrelationId);
            return new AckDto { CorrelationId = x.CorrelationId };
        });
    }
}
