using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Worker;

public class ConnectionHost(IWorkerHubClient hubClient, Application.ISyncContextFactory syncContextFactory, IServiceProvider serviceProvider, ILogger<ConnectionHost> logger) : BackgroundService
{
    private readonly ILogger<ConnectionHost> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async Task<SyncCompletedDto> OnStartSync(StartSyncDto dto)
        {
            _logger.LogInformation("Syncing soooo... hard....");

            var timeRange = new TimeRange(dto.Start, dto.End);
            var accessToken = dto.AccessToken.ToSecureString();

            var syncContext = await syncContextFactory.CreateAsync(dto.ConnectorId, dto.NetworkProperties, timeRange, accessToken, stoppingToken);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var connectorInfo = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
                connectorInfo.Initialize(new ConnectorInfo(dto.ConnectorId, dto.NetworkId));

                var syncContextAccessor = scope.ServiceProvider.GetRequiredService<ISyncContextAccessor>();
                syncContextAccessor.SyncContext = syncContext;

                var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
                var result = await syncService.SyncAsync(syncContext, stoppingToken);
                _logger.LogInformation("Sync completed");

                return new SyncCompletedDto
                {
                    CorrelationId = dto.CorrelationId,
                    ConnectorId = dto.ConnectorId,
                    Start = dto.Start,
                    End = dto.End,
                    TasksCount = result.TasksCount,
                    FailedTasksCount = result.FailedTasksCount,
                    SuccessRate = result.SuccessRate,
                    TotalInteractionsCount = result.TotalInteractionsCount
                };
            };

        }

        async Task OnSetSecrets(SetSecretsDto dto)
        {
            _logger.LogInformation("Setting {count} secrets", dto.Secrets.Count);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var secretRepositoryFactory = scope.ServiceProvider.GetRequiredService<ISecretRepositoryFactory>();
                var secretRepository = secretRepositoryFactory.Create();

                foreach (var secret in dto.Secrets)
                    await secretRepository.SetSecretAsync(secret.Key, secret.Value.ToSecureString(), stoppingToken);
            };

            _logger.LogInformation("Secrets has been set");
        }

        async Task OnRotateSecrets(RotateSecretsDto dto)
        {
            _logger.LogInformation("Rotating secrets for connector '{connectorId}' of type '{type}'", dto.CorrelationId, dto.ConnectorType);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var contextFactory = scope.ServiceProvider.GetRequiredService<ISecretRotationContextFactory>();
                var contextAccesor = scope.ServiceProvider.GetRequiredService<ISecretRotationContextAccessor>();
                var service = scope.ServiceProvider.GetRequiredService<ISecretRotationService>();

                var context = await contextFactory.CreateAsync(dto.ConnectorId, dto.NetworkProperties, stoppingToken);
                contextAccesor.SecretRotationContext = context;
                await service.ExecuteAsync(context, stoppingToken);
            };

            _logger.LogInformation("Secrets has been rotated");
        }

        await hubClient.ConnectAsync(configuration: x =>
        {
            x.OnStartSync = OnStartSync;
            x.OnSetSecrets = OnSetSecrets;
            x.OnRotateSecrets = OnRotateSecrets;
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