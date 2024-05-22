using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;

namespace NetworkPerspective.Sync.Worker;

public class ConnectionHost(IWorkerHubClient hubClient, ILogger<ConnectionHost> logger) : BackgroundService
{
    private readonly ILogger<ConnectionHost> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await hubClient.ConnectAsync(stoppingToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var ping = new PingDto
            {
                CorrelationId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
            };
            _ = await hubClient.PingAsync(ping);

            await Task.Delay(15000, stoppingToken);
        }
    }
}