using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Dtos;
using NetworkPerspective.Sync.Framework.Mappers;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("status")]
    [Authorize]
    public class StatusController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly IStatusService _statusService;
        private readonly IConnectorInfoProvider _connectorInfoProvider;

        public StatusController(IConnectorService connectorService, IStatusService statusService, IConnectorInfoProvider connectorInfoProvider)
        {
            _connectorService = connectorService;
            _statusService = statusService;
            _connectorInfoProvider = connectorInfoProvider;
        }

        /// <summary>
        /// Current network status
        /// </summary>
        /// <param name="stoppingToken">Stopping token</param>
        /// <response code="200">Status</response>
        /// <response code="400">Request cancelled</response>
        /// <response code="401">Missing or invalid authorization token</response>
        /// <response code="404">Network doesn't exist</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(StatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<StatusDto> GetStatus(CancellationToken stoppingToken = default)
        {
            var connectorInfo = _connectorInfoProvider.Get();

            await _connectorService.ValidateExists(connectorInfo.Id, stoppingToken);

            var status = await _statusService.GetStatusAsync(connectorInfo, stoppingToken);

            return StatusMapper.DomainStatusToDto(status);
        }
    }
}