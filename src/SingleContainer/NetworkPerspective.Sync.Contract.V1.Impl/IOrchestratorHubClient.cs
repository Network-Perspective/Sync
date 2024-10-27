using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Exceptions;

using Polly;
using Polly.Retry;

namespace NetworkPerspective.Sync.Contract.V1.Impl;

public interface IOrchestratorHubClient : IOrchestratorClient
{
    Task ConnectAsync(Action<OrchestratorClientConfiguration> configuration = null, Action<IHubConnectionBuilder> connectionConfiguration = null, CancellationToken stoppingToken = default);
}

internal class OrchestartorHubClient : IOrchestratorHubClient
{
    private HubConnection _connection;
    private readonly OrchestratorClientConfiguration _callbacks = new();
    private readonly OrchestratorHubClientConfig _config;
    private readonly ILogger<IOrchestratorHubClient> _logger;
    private readonly AsyncRetryPolicy _asyncRetryPolicy;

    public OrchestartorHubClient(IOptions<OrchestratorHubClientConfig> config, ILogger<IOrchestratorHubClient> logger)
    {
        _config = config.Value;
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

    public async Task<AckDto> SyncCompletedAsync(SyncCompletedDto syncCompleted)
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
        _connection.On<StartSyncDto, AckDto>(nameof(IWorkerClient.SyncAsync), x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to start sync '{connectorId}' from {start} to {end}", x.CorrelationId, x.ConnectorId, x.Start, x.End);

            _ = Task.Run(async () =>
            {
                if (_callbacks.OnStartSync is null)
                    throw new MissingHandlerException(nameof(OrchestratorClientConfiguration.OnStartSync));

                var result = await _callbacks.OnStartSync(x);
                await SyncCompletedAsync(result);
            });

            _logger.LogInformation("Sending ack '{correlationId}'", x.CorrelationId);
            return new AckDto { CorrelationId = x.CorrelationId };
        });

        _connection.On<SetSecretsDto, AckDto>(nameof(IWorkerClient.SetSecretsAsync), async x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to set {count} secrets", x.CorrelationId, x.Secrets.Count);

            if (_callbacks.OnSetSecrets is null)
                throw new MissingHandlerException(nameof(OrchestratorClientConfiguration.OnSetSecrets));

            await _callbacks.OnSetSecrets(x);

            _logger.LogInformation("Sending ack '{correlationId}'", x.CorrelationId);
            return new AckDto { CorrelationId = x.CorrelationId };
        });

        _connection.On<RotateSecretsDto, AckDto>(nameof(IWorkerClient.RotateSecretsAsync), async x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to rotate secrets for connector '{connectorId}' of type '{type}'", x.CorrelationId, x.ConnectorId, x.ConnectorType);

            if (_callbacks.OnRotateSecrets is null)
                throw new MissingHandlerException(nameof(OrchestratorClientConfiguration.OnRotateSecrets));

            await _callbacks.OnRotateSecrets(x);

            _logger.LogInformation("Sending ack '{correlationId}'", x.CorrelationId);
            return new AckDto { CorrelationId = x.CorrelationId };
        });

        _connection.On<GetConnectorStatusDto, ConnectorStatusDto>(nameof(IWorkerClient.GetConnectorStatusAsync), async x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to get connector '{connectorId}' status", x.CorrelationId, x.ConnectorId);

            if (_callbacks.OnGetConnectorStatus is null)
                throw new MissingHandlerException(nameof(OrchestratorClientConfiguration.OnGetConnectorStatus));

            var result = await _callbacks.OnGetConnectorStatus(x);
            _logger.LogInformation("Sending response to request '{correlationId}'", x.CorrelationId);
            return result;
        });

        _connection.On<GetWorkerCapabilitiesDto, WorkerCapabilitiesDto>(nameof(IWorkerClient.GetWorkerCapabilitiesAsync), async x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to get worker capabilities", x.CorrelationId);

            if (_callbacks.OnGetWorkerCapabilities is null)
                throw new MissingHandlerException(nameof(OrchestratorClientConfiguration.OnGetWorkerCapabilities));

            var result = await _callbacks.OnGetWorkerCapabilities(x);
            _logger.LogInformation("Sending response to request '{correlationId}'", x.CorrelationId);
            return result;
        });

        _connection.On<InitializeOAuthRequest, InitializeOAuthResponse>(nameof(IWorkerClient.InitializeOAuthAsync), async x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to initialize OAuth", x.CorrelationId);

            if (_callbacks.OnInitializeOAuth is null)
                throw new MissingHandlerException(nameof(OrchestratorClientConfiguration.OnInitializeOAuth));

            var result = await _callbacks.OnInitializeOAuth(x);
            _logger.LogInformation("Sending response to request '{correlationId}'", x.CorrelationId);
            return result;
        });

        _connection.On<HandleOAuthCallbackRequest, AckDto>(nameof(IWorkerClient.HandleOAuthCallbackAsync), async x =>
        {
            _logger.LogInformation("Received request '{correlationId}' to handle OAuth callback", x.CorrelationId);

            if (_callbacks.OnHandleOAuth is null)
                throw new MissingHandlerException(nameof(OrchestratorClientConfiguration.OnHandleOAuth));

            var result = await _callbacks.OnHandleOAuth(x);
            _logger.LogInformation("Sending response to request '{correlationId}'", x.CorrelationId);
            return result;
        });
    }
}