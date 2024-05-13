﻿using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.Worker;
using NetworkPerspective.Sync.Orchestrator.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Hubs;

[Authorize(AuthenticationSchemes = WorkerAuthOptions.DefaultScheme)]
public class WorkerHubV1 : Hub<IWorkerClient>, IOrchestratorClient
{
    private readonly IConnectionsLookupTable _connectionsLookupTable;
    private readonly ILogger<WorkerHubV1> _logger;

    public WorkerHubV1(IConnectionsLookupTable connectionsLookupTable, ILogger<WorkerHubV1> logger)
    {
        _connectionsLookupTable = connectionsLookupTable;
        _logger = logger;
    }

    public async Task<PongDto> PingAsync(PingDto ping)
    {
        var workerId = Context.GetConnectorId();

        _logger.LogInformation("Received ping from {connectorId}", workerId);
        await Task.Yield();
        return new PongDto { CorrelationId = ping.CorrelationId, PingTimestamp = ping.Timestamp };
    }

    public override async Task OnConnectedAsync()
    {
        var connectorId = Context.GetConnectorId();

        _logger.LogInformation("Worker '{id}' connected", connectorId);
        _connectionsLookupTable.Set(connectorId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var connectorId = Context.GetConnectorId();

        _logger.LogInformation("Worker '{id}' disconnected", connectorId);
        _connectionsLookupTable.Remove(connectorId);

        return base.OnDisconnectedAsync(exception);
    }

    public async Task<AckDto> StartSyncAsync(Guid connectorId, StartSyncDto startSyncRequestDto)
    {
        _logger.LogInformation("Sending request '{correlationId}' to worker '{id}' to start sync...", startSyncRequestDto.CorrelationId, connectorId);
        var connectionId = _connectionsLookupTable.Get(connectorId);
        var response = await Clients.Client(connectionId).StartSyncAsync(startSyncRequestDto);
        _logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
        return response;
    }

    public async Task<AckDto> SyncCompletedAsync(SyncCompletedDto syncCompleted)
    {
        _logger.LogInformation("Received notification from worker '{id}' sync completed", Context.GetConnectorId());

        await Task.Yield();

        return new AckDto { CorrelationId = syncCompleted.CorrelationId };
    }
}