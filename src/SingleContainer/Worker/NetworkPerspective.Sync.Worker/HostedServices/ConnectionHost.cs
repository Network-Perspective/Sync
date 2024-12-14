using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application;

namespace NetworkPerspective.Sync.Worker.HostedServices;

internal class ConnectionHost(IOrchestratorHubClient hubClient, IServiceProvider serviceProvider, IVault secretRepository, ILogger<ConnectionHost> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async Task<string> TokenFactory()
        {
            var nameTask = secretRepository.GetSecretAsync(Keys.OrchestratorClientNameKey, stoppingToken);
            var passTask = secretRepository.GetSecretAsync(Keys.OrchestratorClientSecretKey, stoppingToken);

            await Task.WhenAll(nameTask, passTask);

            var tokenBytes = Encoding.UTF8.GetBytes($"{nameTask.Result.ToSystemString()}:{passTask.Result.ToSystemString()}");
            var tokenBase64 = Convert.ToBase64String(tokenBytes);
            return tokenBase64;
        }

        await hubClient.ConnectAsync(configuration: x =>
        {
            x.TokenFactory = TokenFactory;
        }, stoppingToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Ping();
            await Task.Delay(15000, stoppingToken);
        }
    }
    private async Task Ping()
    {
        try
        {
            var ping = new PingDto
            {
                CorrelationId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
            };
            _ = await hubClient.PingAsync(ping);
        }
        catch (Exception)
        {
            logger.LogWarning("Unable to ping orchestrator");
        }
    }
}