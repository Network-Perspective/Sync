using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS;

using Polly;
using Polly.Retry;

namespace NetworkPerspective.Sync.Contract.V1.Impl;

public interface IOrchestratorHubClient : IOrchestratorClient
{
    Task ConnectAsync(Action<OrchestratorClientConfiguration> configuration = null, Action<IHubConnectionBuilder> connectionConfiguration = null, CancellationToken stoppingToken = default);
}

internal class OrchestratorHubClient : IOrchestratorHubClient
{
    private HubConnection _connection;
    private readonly OrchestratorClientConfiguration _callbacks = new();
    private readonly OrchestratorHubClientConfig _config;
    private readonly IMediator _mediator;
    private readonly ILogger<OrchestratorHubClient> _logger;
    private readonly AsyncRetryPolicy _asyncRetryPolicy;

    public OrchestratorHubClient(IMediator mediator, IOptions<OrchestratorHubClientConfig> config, ILogger<OrchestratorHubClient> logger)
    {
        _config = config.Value;
        _mediator = mediator;
        _logger = logger;

        _asyncRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(_config.Resiliency.Retries,
                (ex, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(ex, "Unable to connect to orchestrator service at '{BaseUrl}'. Next attempt ({RetryCount}) in {Delay}s", _config.BaseUrl, retryCount + 1, timespan.TotalSeconds);
                });
    }

    public async Task ConnectAsync(Action<OrchestratorClientConfiguration> configuration = null, Action<IHubConnectionBuilder> connectionConfiguration = null, CancellationToken stoppingToken = default)
    {
        configuration?.Invoke(_callbacks);

        var hubUrl = $"{_config.BaseUrl}ws/v1/workers-hub";

        var builder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = _callbacks.TokenFactory; // todo: it's getting called twice at startup because of the negotiation protocol request. enforce websocket protocol or implement cache for credentials?
            })
            .WithAutomaticReconnect(_config.Resiliency.Retries);

        connectionConfiguration?.Invoke(builder);

        _connection = builder.Build();
        InitializeConnection();

        await _asyncRetryPolicy.ExecuteAsync((ct) => _connection.StartAsync(ct), stoppingToken);
    }

    public async Task<AckDto> SyncCompletedAsync(SyncResponse syncCompleted)
    {
        return await _asyncRetryPolicy.ExecuteAsync(() => _connection.InvokeAsync<AckDto>(nameof(IOrchestratorClient.SyncCompletedAsync), syncCompleted));
    }

    public async Task<AckDto> AddLogAsync(AddLogDto addLog)
    {
        return await _asyncRetryPolicy.ExecuteAsync(() => _connection.InvokeAsync<AckDto>(nameof(IOrchestratorClient.AddLogAsync), addLog));
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
        _connection.On<SyncRequest, AckDto>(nameof(IWorkerClient.SyncAsync), x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to start sync '{connectorId}' from {start} to {end}", x.CorrelationId, x.Connector.Id, x.Start, x.End);

            _ = Task.Run(async () =>
            {
                var result = await _mediator.SendAsync<SyncRequest, SyncResponse>(x);
                await SyncCompletedAsync(result);
            });

            _logger.LogInformation("Sending ack '{correlationId}'", x.CorrelationId);
            return new AckDto { CorrelationId = x.CorrelationId };
        });

        _connection.On<SetSecretsRequest, AckDto>(nameof(IWorkerClient.SetSecretsAsync),
            x => _mediator.SendAsync<SetSecretsRequest, AckDto>(x));

        _connection.On<RotateSecretsRequest, AckDto>(nameof(IWorkerClient.RotateSecretsAsync),
            x => _mediator.SendAsync<RotateSecretsRequest, AckDto>(x));

        _connection.On<ConnectorStatusRequest, ConnectorStatusResponse>(nameof(IWorkerClient.GetConnectorStatusAsync),
            x => _mediator.SendAsync<ConnectorStatusRequest, ConnectorStatusResponse>(x));

        _connection.On<WorkerCapabilitiesRequest, WorkerCapabilitiesResponse>(nameof(IWorkerClient.GetWorkerCapabilitiesAsync),
            x => _mediator.SendAsync<WorkerCapabilitiesRequest, WorkerCapabilitiesResponse>(x));

        _connection.On<InitializeOAuthRequest, InitializeOAuthResponse>(nameof(IWorkerClient.InitializeOAuthAsync),
            x => _mediator.SendAsync<InitializeOAuthRequest, InitializeOAuthResponse>(x));

        _connection.On<HandleOAuthCallbackRequest, AckDto>(nameof(IWorkerClient.HandleOAuthCallbackAsync),
            x => _mediator.SendAsync<HandleOAuthCallbackRequest, AckDto>(x));
    }
}