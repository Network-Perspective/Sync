using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Hubs;

[Authorize]
public class ConnectorHubV1 : Hub<IConnectorClient>, IOrchestratorClient
{
    private readonly IConnectionsLookupTable _connectionsLookupTable;
    private readonly ILogger<ConnectorHubV1> _logger;

    public ConnectorHubV1(IConnectionsLookupTable connectionsLookupTable, ILogger<ConnectorHubV1> logger)
    {
        _connectionsLookupTable = connectionsLookupTable;
        _logger = logger;
    }

    public async Task<PongDto> PingAsync(PingDto ping)
    {
        var connectorId = Context.GetConnectorId();

        _logger.LogInformation("Received ping from {connectorId}", connectorId);
        await Task.Yield();
        return new PongDto { CorrelationId = ping.CorrelationId, PingTimestamp = ping.Timestamp };
    }

    public override async Task OnConnectedAsync()
    {
        var connectorId = Context.GetConnectorId();

        _logger.LogInformation("Connector '{id}' connected", connectorId);
        _connectionsLookupTable.Set(connectorId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var connectorId = Context.GetConnectorId();

        _logger.LogInformation("Connector '{id}' disconnected", connectorId);
        _connectionsLookupTable.Remove(connectorId);

        return base.OnDisconnectedAsync(exception);
    }

    public async Task<AckDto> StartSyncAsync(Guid connectorId, StartSyncDto startSyncRequestDto)
    {
        _logger.LogInformation("Sending request '{correlationId}' to connector '{id}' to start sync...", startSyncRequestDto.CorrelationId, connectorId);
        var connectionId = _connectionsLookupTable.Get(connectorId);
        var response = await Clients.Client(connectionId).StartSyncAsync(startSyncRequestDto);
        _logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
        return response;
    }

    public async Task<AckDto> SyncCompletedAsync(SyncCompletedDto syncCompleted)
    {
        _logger.LogInformation("Received notification from connector '{id}' sync completed", Context.GetConnectorId());

        await Task.Yield();

        return new AckDto { CorrelationId = syncCompleted.CorrelationId };
    }
}
