using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.Worker;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1;

[Authorize(AuthenticationSchemes = WorkerAuthOptions.DefaultScheme)]
public class WorkerHubV1(IConnectionsLookupTable connectionsLookupTable, IStatusLogger statusLogger, IServiceProvider serviceProvider, IClock clock, ILogger<WorkerHubV1> logger) : Hub<IWorkerClient>, IOrchestratorClient
{
    public override async Task OnConnectedAsync()
    {
        var workerName = Context.GetWorkerName();
        logger.LogInformation("Worker '{name}' connected", workerName);

        var workerConnection = new WorkerConnection(workerName, Context.ConnectionId);
        connectionsLookupTable.Set(workerName, workerConnection);
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var workerName = Context.GetWorkerName();

        logger.LogInformation("Worker '{name}' disconnected", workerName);
        connectionsLookupTable.Remove(workerName);

        return base.OnDisconnectedAsync(exception);
    }

    public async Task<AckDto> SyncCompletedAsync(SyncResponse dto)
    {
        logger.LogInformation("Received notification from worker '{id}' sync completed", Context.GetWorkerName());

        var now = clock.UtcNow();
        var timeRange = new TimeRange(dto.Start, dto.End);
        var log = SyncHistoryEntry.Create(dto.ConnectorId, now, timeRange, dto.SuccessRate, dto.TasksCount, dto.TotalInteractionsCount);

        await using var scope = serviceProvider.CreateAsyncScope();
        var syncHistoryService = scope.ServiceProvider.GetService<ISyncHistoryService>();
        await syncHistoryService.SaveLogAsync(log);

        return new AckDto { CorrelationId = dto.CorrelationId };
    }

    public async Task<PongDto> PingAsync(PingDto ping)
    {
        var workerName = Context.GetWorkerName();

        logger.LogInformation("Received ping from {connectorId}", workerName);
        await Task.Yield();
        return new PongDto { CorrelationId = ping.CorrelationId, PingTimestamp = ping.Timestamp };
    }

    public async Task<AckDto> AddLogAsync(AddLogDto dto)
    {
        if (dto.ConnectorId == Guid.Empty)
        {
            // TODO worker-scoped logs
            logger.LogWarning("Received request to set worker-scoped status log. Currently only connector-scoped status logs are handled. Igrnoring.");
        }
        else
        {
            var domainStatusLogLevel = ToDomainStatusLogLevel(dto.Level);
            await statusLogger.AddLogAsync(dto.ConnectorId, dto.Message, domainStatusLogLevel);
        }

        return new AckDto { CorrelationId = dto.CorrelationId };
    }

    private static Application.Domain.StatusLogLevel ToDomainStatusLogLevel(Contract.V1.Dtos.StatusLogLevel level)
        => level switch
        {
            Contract.V1.Dtos.StatusLogLevel.Error => Application.Domain.StatusLogLevel.Error,
            Contract.V1.Dtos.StatusLogLevel.Warning => Application.Domain.StatusLogLevel.Warning,
            Contract.V1.Dtos.StatusLogLevel.Debug => Application.Domain.StatusLogLevel.Debug,
            _ => Application.Domain.StatusLogLevel.Info,
        };
}