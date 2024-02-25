using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Dtos;
using NetworkPerspective.Sync.Framework.Mappers;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("status")]
    [Authorize]
    public class StatusController : ControllerBase
    {
        private readonly INetworkService _networkService;
        private readonly IStatusService _statusService;
        private readonly INetworkIdProvider _networkIdProvider;

        public StatusController(INetworkService networkService, IStatusService statusService, INetworkIdProvider networkIdProvider)
        {
            _networkService = networkService;
            _statusService = statusService;
            _networkIdProvider = networkIdProvider;
        }

        /// <summary>
        /// Current network status
        /// </summary>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Status</returns>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "Status", typeof(StatusDto))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Request cancelled")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Network doesn't exist")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<StatusDto> GetStatus(CancellationToken stoppingToken = default)
        {
            await _networkService.ValidateExists(_networkIdProvider.Get(), stoppingToken);

            var status = await _statusService.GetStatusAsync(_networkIdProvider.Get(), stoppingToken);

            return StatusMapper.DomainStatusToDto(status);
        }
    }
}