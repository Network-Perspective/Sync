using System;
using System.Threading;
using System.Threading.Tasks;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route("api/connectors/{connectorId:guid}/status")]
[Authorize]
public class StatusController(IStatusService statusService) : ControllerBase
{
    /// <summary>
    /// Connector status
    /// </summary>
    /// <param name="connectorId">Connector Id</param>
    /// <param name="logsSeverity">Filter logs severity level</param>
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
    public async Task<StatusDto> GetStatus([FromRoute] Guid connectorId, [FromQuery] StatusLogLevel logsSeverity = StatusLogLevel.Info, CancellationToken stoppingToken = default)
    {
        var status = await statusService.GetStatusAsync(connectorId, logsSeverity, stoppingToken);

        return status.Adapt<StatusDto>();
    }
}