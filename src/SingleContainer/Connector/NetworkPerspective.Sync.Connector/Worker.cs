using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Connector;

public class Worker(HubClient hubClient, ILogger<Worker> logger) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await hubClient.ConnectAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var ping = new PingDto 
            { 
                CorrelationId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
            };
            _ = await hubClient.PingAsync(ping);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
