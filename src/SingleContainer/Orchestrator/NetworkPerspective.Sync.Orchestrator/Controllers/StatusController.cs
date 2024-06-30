using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Dtos;
using NetworkPerspective.Sync.Orchestrator.Mappers;

namespace NetworkPerspective.Sync.Framework.Controllers;

[Route("api/connectors/{connectorId:guid}/status")]
[Authorize]
public class StatusController : ControllerBase
{
    private readonly IStatusService _statusService;

    public StatusController(IStatusService statusService)
    {
        _statusService = statusService;
    }

    /// <summary>
    /// Connector status
    /// </summary>
    /// <param name="connectorId">Connector Id</param>
    /// <param name="stoppingToken">Stopping token</param>
    /// <response code="200">Status</response>
    /// <response code="400">Request cancelled</response>
    /// <response code="401">Missing or invalid authorization token</response>
    /// <response code="404">Connector doesn't exist</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(StatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<StatusDto> GetStatus([FromRoute] Guid connectorId, CancellationToken stoppingToken = default)
    {
        var status = await _statusService.GetStatusAsync(connectorId, stoppingToken);

        return StatusMapper.DomainStatusToDto(status);
    }
}