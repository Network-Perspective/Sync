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
    Task ConnectAsync(Action<OrchestratorClientConfiguration> configuration = null, Action<IHubConnectionBuilder> connectionConfiguration = null, CancellationToken stoppingToken = default);
}

internal class WorkerHubClient : IWorkerHubClient
{
    private HubConnection _connection;
    private readonly OrchestratorClientConfiguration _callbacks = new();
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

    public async Task ConnectAsync(Action<OrchestratorClientConfiguration> configuration = null, Action<IHubConnectionBuilder> connectionConfiguration = null, CancellationToken stoppingToken = default)
    {
        configuration?.Invoke(_callbacks);

        var hubUrl = $"{_config.BaseUrl}ws/v1/workers-hub";

        var builder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = TokenFactory;
            })
            .WithAutomaticReconnect();

        connectionConfiguration?.Invoke(builder);

        _connection = builder.Build();
        InitializeConnection();

        await _connection.StartAsync(stoppingToken);
    }

    public Task<AckDto> SyncCompletedAsync(SyncCompletedDto syncCompleted)
    {
        return _connection.InvokeAsync<AckDto>(nameof(IOrchestratorClient.SyncCompletedAsync), syncCompleted);
    }

    public Task<AckDto> AddLogAsync(AddLogDto addLog)
    {
        return _connection.InvokeAsync<AckDto>(nameof(IOrchestratorClient.AddLogAsync), addLog);
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
        _connection.On<StartSyncDto, AckDto>(nameof(IWorkerClient.StartSyncAsync), x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to start sync '{connectorId}' from {start} to {end}", x.CorrelationId, x.ConnectorId, x.Start, x.End);

            _ = Task.Run(async () =>
            {
                if (_callbacks.OnStartSync is not null)
                {
                    var result = await _callbacks.OnStartSync(x);
                    await SyncCompletedAsync(result);
                }
            });

            _logger.LogInformation("Sending ack '{correlationId}'", x.CorrelationId);
            return new AckDto { CorrelationId = x.CorrelationId };
        });

        _connection.On<SetSecretsDto, AckDto>(nameof(IWorkerClient.SetSecretsAsync), async x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to set {count} secrets", x.CorrelationId, x.Secrets.Count);

            if (_callbacks.OnSetSecret is not null)
                await _callbacks.OnSetSecret(x);

            _logger.LogInformation("Sending ack '{correlationId}'", x.CorrelationId);
            return new AckDto { CorrelationId = x.CorrelationId };
        });
    }
}