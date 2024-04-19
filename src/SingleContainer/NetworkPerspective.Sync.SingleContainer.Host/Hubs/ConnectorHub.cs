using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract;

namespace NetworkPerspective.Sync.Orchestrator.Hubs
{
    [Authorize]
    public class ConnectorHub : Hub<IConnectorClient>, IOrchestratorClient
    {
        private readonly ILogger<ConnectorHub> _logger;

        public ConnectorHub(ILogger<ConnectorHub> logger)
        {
            _logger = logger;
        }

        public override async  Task OnConnectedAsync()
        {
            foreach(var claim in Context.User.Claims)
            {
                _logger.LogInformation($"{claim.Type}: {claim.Value}");
            }
            await base.OnConnectedAsync();

        }

        public async Task<AckResponseDto> RegisterConnectorAsync(RegisterConnectorRequestDto registerConnectorDto)
        {
            _logger.LogInformation("Registered connector: {asd}", Context.User);
            _logger.LogInformation("CorrelationId: {Id}", registerConnectorDto.CorrelationId);
            await StartSyncAsync(new Guid(Context.UserIdentifier), new StartSyncRequestDto() { CorrelationId = Guid.NewGuid() });
            return new AckResponseDto {  CorrelationId = registerConnectorDto.CorrelationId};
        }

        public async Task<AckResponseDto> StartSyncAsync(Guid connectorId, StartSyncRequestDto startSyncRequestDto)
        {
            await Clients.User(connectorId.ToString()).StartSyncAsync(startSyncRequestDto);
            return new AckResponseDto { CorrelationId = connectorId };
        }
    }
}
