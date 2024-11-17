using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.HostedServices;

internal class ConnectionHost(IOrchestratorHubClient hubClient, ISyncContextFactory syncContextFactory, IServiceProvider serviceProvider, IVault secretRepository, ILogger<ConnectionHost> logger) : BackgroundService
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

        async Task<SyncCompletedDto> OnStartSync(StartSyncDto dto)
        {
            logger.LogInformation("Syncing started for connector '{connectorId}'", dto.Connector.Id);

            var timeRange = new TimeRange(dto.Start, dto.End);
            var accessToken = dto.AccessToken.ToSecureString();

            var syncContext = await syncContextFactory.CreateAsync(dto.Connector.Id, dto.Connector.Type, dto.Connector.Properties, timeRange, accessToken, stoppingToken);

            if (dto.Employees is not null)
                syncContext.Set(dto.Employees);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var connectorInfo = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
                connectorInfo.Initialize(new ConnectorInfo(dto.Connector.Id, dto.Connector.Type, dto.Connector.Properties));

                var syncContextAccessor = scope.ServiceProvider.GetRequiredService<ISyncContextAccessor>();
                syncContextAccessor.SyncContext = syncContext;

                var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
                var result = await syncService.SyncAsync(syncContext, stoppingToken);
                logger.LogInformation("Sync for connector '{connectorId}' completed", dto.Connector.Id);

                return new SyncCompletedDto
                {
                    CorrelationId = dto.CorrelationId,
                    ConnectorId = dto.Connector.Id,
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
            logger.LogInformation("Setting {count} secrets", dto.Secrets.Count);

            foreach (var secret in dto.Secrets)
                await secretRepository.SetSecretAsync(secret.Key, secret.Value.ToSecureString(), stoppingToken);

            logger.LogInformation("Secrets has been set");
        }

        async Task OnRotateSecrets(RotateSecretsDto dto)
        {
            logger.LogInformation("Rotating secrets for connector '{connectorId}' of type '{type}'", dto.Connector.Id, dto.Connector.Type);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var connectorInfo = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
                connectorInfo.Initialize(new ConnectorInfo(dto.Connector.Id, dto.Connector.Type, dto.Connector.Properties));

                var service = scope.ServiceProvider.GetRequiredService<ISecretRotationService>();
                await service.ExecuteAsync(stoppingToken);
            };

            logger.LogInformation("Secrets has been rotated");
        }

        async Task<ConnectorStatusDto> OnGetConnectorStatus(GetConnectorStatusDto dto)
        {
            logger.LogInformation("Checking connector '{connectorId}' status", dto.Connector.Id);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var connectorInfo = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
                connectorInfo.Initialize(new ConnectorInfo(dto.Connector.Id, dto.Connector.Type, dto.Connector.Properties));

                var authTester = scope.ServiceProvider.GetRequiredService<IAuthTester>();
                var isAuthorized = await authTester.IsAuthorizedAsync(stoppingToken);

                var taskStatusCache = scope.ServiceProvider.GetRequiredService<ITasksStatusesCache>();
                var taskStatus = await taskStatusCache.GetStatusAsync(dto.Connector.Id, stoppingToken);

                var isRunning = taskStatus != SingleTaskStatus.Empty;

                logger.LogInformation("Status check for connector '{connectorId}' completed", dto.Connector.Id);

                return new ConnectorStatusDto
                {
                    CorrelationId = dto.Connector.Id,
                    IsAuthorized = isAuthorized,
                    IsRunning = isRunning,
                    CurrentTaskCaption = taskStatus.Caption,
                    CurrentTaskDescription = taskStatus.Description,
                    CurrentTaskCompletionRate = taskStatus.CompletionRate
                };
            };
        }

        async Task<WorkerCapabilitiesDto> OnGetWorkerCapabilities(GetWorkerCapabilitiesDto dto)
        {
            logger.LogInformation("Checking worker capabilities");

            var capabilitiesService = serviceProvider.GetRequiredService<ICapabilitiesService>();
            var connectorTypes = await capabilitiesService.GetSupportedConnectorTypesAsync(stoppingToken);
            return new WorkerCapabilitiesDto
            {
                CorrelationId = dto.CorrelationId,
                SupportedConnectorTypes = connectorTypes.Select(x => x.Name)
            };
        }

        async Task<InitializeOAuthResponse> OnInitializeOAuth(InitializeOAuthRequest dto)
        {
            logger.LogInformation("Initializing OAuth for connector '{connectorId}' (of type '{connectorType}')", dto.Connector.Id, dto.Connector.Type);

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var connectorInfoInitializer = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
                var connectorInfo = new ConnectorInfo(dto.Connector.Id, dto.Connector.Type, dto.Connector.Properties);
                connectorInfoInitializer.Initialize(connectorInfo);

                var authService = scope.ServiceProvider.GetRequiredService<IOAuthService>();

                var context = new OAuthContext(connectorInfo, dto.CallbackUri);

                var result = await authService.InitializeOAuthAsync(context, stoppingToken);

                var response = new InitializeOAuthResponse
                {
                    CorrelationId = dto.CorrelationId,
                    AuthUri = result.AuthUri,
                    State = result.State,
                    StateExpirationTimestamp = result.StateExpirationTimestamp
                };

                return response;
            }
        }

        async Task<AckDto> OnHandleOAuth(HandleOAuthCallbackRequest dto)
        {
            logger.LogInformation("Handling OAuth callback");

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

                if (!cache.TryGetValue(dto.State, out OAuthContext context))
                    throw new OAuthException("State does not match initialized value");

                var connectorInfo = scope.ServiceProvider.GetRequiredService<IConnectorInfoInitializer>();
                connectorInfo.Initialize(context.Connector);

                var authService = scope.ServiceProvider.GetRequiredService<IOAuthService>();

                await authService.HandleAuthorizationCodeCallbackAsync(dto.Code, context, stoppingToken);

                return new AckDto { CorrelationId = dto.CorrelationId };
            }
        }

        await hubClient.ConnectAsync(configuration: x =>
        {
            x.TokenFactory = TokenFactory;
            x.OnStartSync = OnStartSync;
            x.OnSetSecrets = OnSetSecrets;
            x.OnRotateSecrets = OnRotateSecrets;
            x.OnGetConnectorStatus = OnGetConnectorStatus;
            x.OnGetWorkerCapabilities = OnGetWorkerCapabilities;
            x.OnInitializeOAuth = OnInitializeOAuth;
            x.OnHandleOAuth = OnHandleOAuth;
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