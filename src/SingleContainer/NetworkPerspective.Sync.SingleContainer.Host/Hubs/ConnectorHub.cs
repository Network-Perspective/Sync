using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract;
using NetworkPerspective.Sync.Contract.Dtos;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Orchestrator.Services;

namespace NetworkPerspective.Sync.Orchestrator.Hubs;

[Authorize]
public class ConnectorHub : Hub<IConnectorClient>, IOrchestratorClient
{
    private readonly IConnectionsLookupTable _connectionsLookupTable;
    private readonly ILogger<ConnectorHub> _logger;

    public ConnectorHub(IConnectionsLookupTable connectionsLookupTable, ILogger<ConnectorHub> logger)
    {
        _connectionsLookupTable = connectionsLookupTable;
        _logger = logger;
    }

    public override async  Task OnConnectedAsync()
    {
        _connectionsLookupTable.Set(Context.GetConnectorId(), Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _connectionsLookupTable.Remove(Context.GetConnectorId());

        return base.OnDisconnectedAsync(exception);
    }

    public async Task<AckResponseDto> RegisterConnectorAsync(RegisterConnectorRequestDto registerConnectorDto)
    {
        _logger.LogInformation("Registered connector: {asd}", Context.User);
        _logger.LogInformation("CorrelationId: {Id}", registerConnectorDto.CorrelationId);
        await Task.Yield();
        //await StartSyncAsync(new Guid(Context.UserIdentifier), new StartSyncRequestDto() { CorrelationId = Guid.NewGuid() });
        return new AckResponseDto {  CorrelationId = registerConnectorDto.CorrelationId};
    }

    public async Task<AckResponseDto> StartSyncAsync(Guid connectorId, StartSyncRequestDto startSyncRequestDto)
    {
        _logger.LogInformation("Sending request '{correlationId}' to connector '{id}' to start sync...", startSyncRequestDto.CorrelationId, connectorId);
        var connectionId = _connectionsLookupTable.Get(connectorId);
        var response = await Clients.Client(connectionId).StartSyncAsync(startSyncRequestDto);
        _logger.LogInformation("Received ack '{correlationId}'", response.CorrelationId);
        return response;
    }
}
