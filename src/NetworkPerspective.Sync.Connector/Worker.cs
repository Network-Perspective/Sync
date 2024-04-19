using NetworkPerspective.Sync.Contract;

namespace NetworkPerspective.Sync.Connector;

public class Worker(HubClient hubClient, ILogger<Worker> logger) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await hubClient.ConnectAsync(stoppingToken);

        var result = await hubClient.RegisterConnectorAsync(new RegisterConnectorRequestDto {  CorrelationId = Guid.NewGuid()});

        _logger.LogInformation("Response CorrelationId: {id}", result.CorrelationId);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
