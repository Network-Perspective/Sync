using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker;

public class ConnectionHost(IWorkerHubClient hubClient, ISyncContextFactory syncContextFactory, IServiceProvider serviceProvider, IVault secretRepository, ILogger<ConnectionHost> logger) : BackgroundService
{
    private readonly ILogger<ConnectionHost> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async Task<string> TokenFactory()
        {
            var name = await secretRepository.GetSecretAsync("orchestrator-client-name", stoppingToken);
            var pass = await secretRepository.GetSecretAsync("orchestrator-client-secret", stoppingToken);
            var tokenBytes = Encoding.UTF8.GetBytes($"{name.ToSystemString()}:{pass.ToSystemString()}");
            var tokenBase64 = Convert.ToBase64String(tokenBytes);
            return tokenBase64;
        }

        async Task<SyncCompletedDto> OnStartSync(StartSyncDto dto)
        {
            _logger.LogInformation("Syncing started for connector '{connectorId}'", dto.ConnectorId);

            var timeRange = new TimeRange(dto.Start, dto.End);
            var accessToken = dto.AccessToken.ToSecureString();

            var syncContext = await syncContextFactory.CreateAsync(dto.ConnectorId, dto.ConnectorType, dto.NetworkProperties, timeRange, accessToken, stoppingToken);

            if (dto.Employees is not null)
                syncContext.Set(dto.Employees);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var connectorInfo = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
                connectorInfo.Initialize(new ConnectorInfo(dto.ConnectorId, dto.NetworkId));

                var syncContextAccessor = scope.ServiceProvider.GetRequiredService<ISyncContextAccessor>();
                syncContextAccessor.SyncContext = syncContext;

                var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
                var result = await syncService.SyncAsync(syncContext, stoppingToken);
                _logger.LogInformation("Sync for connector '{connectorId}' completed", dto.ConnectorId);

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

            foreach (var secret in dto.Secrets)
                await secretRepository.SetSecretAsync(secret.Key, secret.Value.ToSecureString(), stoppingToken);

            _logger.LogInformation("Secrets has been set");
        }

        async Task OnRotateSecrets(RotateSecretsDto dto)
        {
            _logger.LogInformation("Rotating secrets for connector '{connectorId}' of type '{type}'", dto.ConnectorId, dto.ConnectorType);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var contextFactory = scope.ServiceProvider.GetRequiredService<ISecretRotationContextFactory>();
                var contextAccesor = scope.ServiceProvider.GetRequiredService<ISecretRotationContextAccessor>();
                var service = scope.ServiceProvider.GetRequiredService<ISecretRotationService>();

                var context = contextFactory.Create(dto.ConnectorId, dto.NetworkProperties);
                contextAccesor.SecretRotationContext = context;
                await service.ExecuteAsync(context, stoppingToken);
            };

            _logger.LogInformation("Secrets has been rotated");
        }

        async Task<ConnectorStatusDto> OnGetConnectorStatus(GetConnectorStatusDto dto)
        {
            _logger.LogInformation("Checking connector '{connectorID}' status", dto.ConnectorId);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var contextFactory = scope.ServiceProvider.GetRequiredService<IAuthTesterContextFactory>();
                var contextAccesor = scope.ServiceProvider.GetRequiredService<IAuthTesterContextAccessor>();

                var context = contextFactory.Create(dto.ConnectorId, dto.ConnectorType, dto.ConnectorProperties);
                contextAccesor.Context = context;

                var connectorInfo = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
                connectorInfo.Initialize(new ConnectorInfo(dto.ConnectorId, dto.NetworkId));

                var authTester = scope.ServiceProvider.GetRequiredService<IAuthTester>();
                var isAuthorized = await authTester.IsAuthorizedAsync(dto.ConnectorProperties, stoppingToken);

                var taskStatusCache = scope.ServiceProvider.GetRequiredService<ITasksStatusesCache>();
                var taskStatus = await taskStatusCache.GetStatusAsync(dto.ConnectorId, stoppingToken);

                var isRunning = !ReferenceEquals(taskStatus, SingleTaskStatus.Empty); // check if simple '==' is not enough here

                _logger.LogInformation("Status check for connector '{connectorId}' completed", dto.ConnectorId);

                return new ConnectorStatusDto
                {
                    IsAuthorized = isAuthorized,
                    IsRunning = isRunning,
                    CurrentTaskCaption = taskStatus.Caption,
                    CurrentTaskDescription = taskStatus.Description,
                    CurrentTaskCompletionRate = taskStatus.CompletionRate
                };
            };

        }

        await hubClient.ConnectAsync(configuration: x =>
        {
            x.TokenFactory = TokenFactory;
            x.OnStartSync = OnStartSync;
            x.OnSetSecrets = OnSetSecrets;
            x.OnRotateSecrets = OnRotateSecrets;
            x.OnGetConnectrStatus = OnGetConnectorStatus;
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
            _logger.LogWarning("Unable to ping orchestrator");
        }
    }
}