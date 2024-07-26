using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.Worker;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1;

[Authorize(AuthenticationSchemes = WorkerAuthOptions.DefaultScheme)]
public class WorkerHubV1 : Hub<IWorkerClient>, IOrchestratorClient, IWorkerRouter
{
    private readonly IConnectionsLookupTable _connectionsLookupTable;
    private readonly IStatusLogger _statusLogger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IClock _clock;
    private readonly ILogger<WorkerHubV1> _logger;

    public WorkerHubV1(IConnectionsLookupTable connectionsLookupTable, IStatusLogger statusLogger, IServiceProvider serviceProvider, IClock clock, ILogger<WorkerHubV1> logger)
    {
        _connectionsLookupTable = connectionsLookupTable;
        _statusLogger = statusLogger;
        _serviceProvider = serviceProvider;
        _clock = clock;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var workerName = Context.GetWorkerName();

        _logger.LogInformation("Worker '{id}' connected", workerName);
        _connectionsLookupTable.Set(workerName, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var connectorId = Context.GetWorkerName();

        _logger.LogInformation("Worker '{id}' disconnected", connectorId);
        _connectionsLookupTable.Remove(connectorId);

        return base.OnDisconnectedAsync(exception);
    }

    public async Task StartSyncAsync(string workerName, SyncContext syncContext)
    {
        var dto = syncContext.Adapt<StartSyncDto>();
        _logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to start sync...", dto.CorrelationId, workerName);
        var connectionId = _connectionsLookupTable.Get(workerName);
        var response = await Clients.Client(connectionId).SyncAsync(dto);
        _logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task SetSecretsAsync(string workerName, IDictionary<string, SecureString> secrets)
    {
        var dto = new SetSecretsDto
        {
            CorrelationId = Guid.NewGuid(),
            Secrets = secrets.ToDictionary(x => x.Key, x => x.Value.ToSystemString())
        };
        _logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to set secrets...", dto.CorrelationId, workerName);
        var connectionId = _connectionsLookupTable.Get(workerName);
        var response = await Clients.Client(connectionId).SetSecretsAsync(dto);
        _logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task RotateSecretsAsync(string workerName, Guid connectorId, IDictionary<string, string> networkProperties, string connectorType)
    {
        var dto = new RotateSecretsDto
        {
            CorrelationId = Guid.NewGuid(),
            ConnectorId = connectorId,
            NetworkProperties = networkProperties,
            ConnectorType = connectorType
        };
        var connectionId = _connectionsLookupTable.Get(workerName);
        var response = await Clients.Client(connectionId).RotateSecretsAsync(dto);
        _logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task<AckDto> SyncCompletedAsync(SyncCompletedDto dto)
    {
        _logger.LogInformation("Received notification from worker '{id}' sync completed", Context.GetWorkerName());

        var now = _clock.UtcNow();
        var timeRange = new TimeRange(dto.Start, dto.End);
        var log = SyncHistoryEntry.Create(dto.ConnectorId, now, timeRange, dto.SuccessRate, dto.TasksCount, dto.TotalInteractionsCount);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var syncHistoryService = scope.ServiceProvider.GetService<ISyncHistoryService>();
        await syncHistoryService.SaveLogAsync(log);

        return new AckDto { CorrelationId = dto.CorrelationId };
    }

    public async Task<PongDto> PingAsync(PingDto ping)
    {
        var workerName = Context.GetWorkerName();

        _logger.LogInformation("Received ping from {connectorId}", workerName);
        await Task.Yield();
        return new PongDto { CorrelationId = ping.CorrelationId, PingTimestamp = ping.Timestamp };
    }

    public async Task<AckDto> AddLogAsync(AddLogDto dto)
    {
        var domainStatusLogLevel = ToDomainStatusLogLevel(dto.Level);
        await _statusLogger.AddLogAsync(dto.ConnectorId, dto.Message, domainStatusLogLevel);
        return new AckDto { CorrelationId = dto.CorrelationId };
    }

    private Sync.Application.Domain.Statuses.StatusLogLevel ToDomainStatusLogLevel(StatusLogLevel level)
    {
        return level switch
        {
            StatusLogLevel.Error => Sync.Application.Domain.Statuses.StatusLogLevel.Error,
            StatusLogLevel.Warning => Sync.Application.Domain.Statuses.StatusLogLevel.Warning,
            _ => Sync.Application.Domain.Statuses.StatusLogLevel.Info,
        };
    }
}