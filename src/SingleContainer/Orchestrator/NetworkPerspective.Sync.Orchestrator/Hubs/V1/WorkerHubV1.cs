using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.Worker;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1;

[Authorize(AuthenticationSchemes = WorkerAuthOptions.DefaultScheme)]
public class WorkerHubV1 : Hub<IWorkerClient>, IOrchestratorClient, IWorkerRouter
{
    private readonly IConnectionsLookupTable _connectionsLookupTable;
    private readonly ILogger<WorkerHubV1> _logger;

    public WorkerHubV1(IConnectionsLookupTable connectionsLookupTable, ILogger<WorkerHubV1> logger)
    {
        _connectionsLookupTable = connectionsLookupTable;
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
        var dto = StartSyncMapper.ToDto(syncContext);
        _logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to start sync...", dto.CorrelationId, workerName);
        var connectionId = _connectionsLookupTable.Get(workerName);
        var response = await Clients.Client(connectionId).StartSyncAsync(dto);
        _logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
    }

    public async Task<AckDto> SyncCompletedAsync(SyncCompletedDto syncCompleted)
    {
        _logger.LogInformation("Received notification from worker '{id}' sync completed", Context.GetWorkerName());

        await Task.Yield();

        return new AckDto { CorrelationId = syncCompleted.CorrelationId };
    }

    public async Task<PongDto> PingAsync(PingDto ping)
    {
        var workerName = Context.GetWorkerName();

        _logger.LogInformation("Received ping from {connectorId}", workerName);
        await Task.Yield();
        return new PongDto { CorrelationId = ping.CorrelationId, PingTimestamp = ping.Timestamp };
    }
}