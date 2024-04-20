using Microsoft.AspNetCore.SignalR.Client;

using NetworkPerspective.Sync.Contract;
using NetworkPerspective.Sync.Contract.Dtos;

namespace NetworkPerspective.Sync.Connector;

public class HubClient : IOrchestratorClient
{
    private readonly HubConnection _connection;
    private readonly ILogger<HubClient> _logger;

    public HubClient(ILogger<HubClient> logger)
    {
        _logger = logger;

        var hubUrl = "https://localhost:7191/connector-hub";

        static Task<string?> TokenFactory()
        {
            return Task.FromResult<string?>("blablabla");
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = TokenFactory;
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<StartSyncRequestDto, AckResponseDto>(nameof(IConnectorClient.StartSyncAsync), async x =>
        {
            _logger.LogInformation("Received request to start sync '{correlationId}'", x.CorrelationId);

            // Magic placeholder
            // ...
            await Task.Yield();

            _logger.LogInformation("Sending ack '{correlationId}'", x.CorrelationId);
            return new AckResponseDto { CorrelationId = x.CorrelationId };
        });

    }

    public Task ConnectAsync(CancellationToken stoppingToken = default)
        => _connection.StartAsync(stoppingToken);

    public Task<AckResponseDto> RegisterConnectorAsync(RegisterConnectorRequestDto registerConnectorDto)
    {
        return _connection.InvokeAsync<AckResponseDto>(nameof(IOrchestratorClient.RegisterConnectorAsync), registerConnectorDto);
    }
}
