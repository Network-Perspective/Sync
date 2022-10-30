using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Dtos;
using NetworkPerspective.Sync.Framework.Mappers;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("status")]
    public class StatusController : ApiControllerBase
    {
        private readonly INetworkService _networkService;
        private readonly IStatusService _statusService;

        public StatusController(INetworkPerspectiveCore networkPerspectiveCore, INetworkService networkService, IStatusService statusService) : base(networkPerspectiveCore)
        {
            _networkService = networkService;
            _statusService = statusService;
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
            var tokenValidationResponse = await ValidateTokenAsync(stoppingToken);
            await _networkService.ValidateExists(tokenValidationResponse.NetworkId, stoppingToken);

            var status = await _statusService.GetStatusAsync(tokenValidationResponse.NetworkId, stoppingToken);

            return StatusMapper.DomainStatusToDto(status);
        }
    }
}