using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Worker.Application;

namespace NetworkPerspective.Sync.Worker;

public class ConnectionHost(IWorkerHubClient hubClient, ISyncServiceFactory syncServiceFactory, ILogger<ConnectionHost> logger) : BackgroundService
{
    private readonly ILogger<ConnectionHost> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async Task OnStartSync(StartSyncDto startSyncDto)
        {
            _logger.LogInformation("Syncing soooo... hard....");

            var syncService = await syncServiceFactory.CreateAsync(stoppingToken);
            await syncService.SyncAsync(null, stoppingToken);

            _logger.LogInformation("Sync completed");
        }

        async Task OnSetSecrets(SetSecretsDto startSyncDto)
        {
            _logger.LogInformation("Setting secrets");
            await Task.Delay(1000);
            _logger.LogInformation("Secrets has been set");
        }

        await hubClient.ConnectAsync(configuration: x =>
        {
            x.OnStartSync = OnStartSync;
            x.OnSetSecret = OnSetSecrets;
        }, stoppingToken: stoppingToken);

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