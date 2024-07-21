using System;
using System.Threading;
using System.Threading.Tasks;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;
using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route("api/connectors/{connectorId:guid}/sync")]
[Authorize(AuthenticationSchemes = ApiKeyAuthOptions.DefaultScheme)]
public class SyncController(IConnectorsService connectorsService, IWorkerRouter workerRouter) : ControllerBase
{
    /// <summary>
    /// Sync
    /// </summary>
    /// <param name="connectorId"></param>
    /// <param name="dto"></param>
    /// <param name="stoppingToken"></param>
    /// <response code="200">Sync completed</response>
    /// <response code="400">Cannot complete sync due to problem with input data</response>
    /// <response code="401">Missing or invalid authorization token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SyncAsync([FromRoute] Guid connectorId, [FromBody] SyncRequestDto dto, CancellationToken stoppingToken = default)
    {
        var connector = await connectorsService.GetAsync(connectorId, stoppingToken);

        if (connector.Type != "Excel")
            return BadRequest("Sync is available only for 'Excel' connectors");

        var syncRequest = dto.Adapt<SyncRequest>();
        syncRequest.ConnectorId = connectorId;

        // TODO add constraints - minimum numer of emplyees

        await workerRouter.PushSyncAsync(connector.Worker.Name, syncRequest);

        return Ok();
    }
}