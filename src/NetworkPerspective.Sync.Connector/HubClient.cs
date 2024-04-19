using Microsoft.AspNetCore.SignalR.Client;

using NetworkPerspective.Sync.Contract;

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
            return Task.FromResult<string?>("who-the-fuck-is-alice");
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = TokenFactory;
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<StartSyncRequestDto>(nameof(IConnectorClient.StartSyncAsync), x =>
        {
            _logger.LogInformation(x.CorrelationId.ToString());
        });

    }

    public Task ConnectAsync(CancellationToken stoppingToken = default)
        => _connection.StartAsync(stoppingToken);

    public Task<AckResponseDto> RegisterConnectorAsync(RegisterConnectorRequestDto registerConnectorDto)
    {
        return _connection.InvokeAsync<AckResponseDto>(nameof(IOrchestratorClient.RegisterConnectorAsync), registerConnectorDto);
    }
}
