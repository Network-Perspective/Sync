using System;
using System.Collections.Generic;
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
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route("api/connectors/{connectorId:guid}/sync")]
[Authorize(AuthenticationSchemes = ApiKeyAuthOptions.DefaultScheme)]
public class SyncController(IConnectorsService connectorsService, ITokenService tokenService, ISyncHistoryService syncHistoryService, IClock clock, IWorkerRouter workerRouter) : ControllerBase
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
        var nextSyncStart = await syncHistoryService.EvaluateSyncStartAsync(connectorId, stoppingToken);

        var connector = await connectorsService.GetAsync(connectorId, stoppingToken);
        var accessToken = await tokenService.GetAsync(connector.Id, stoppingToken);

        if (connector.Type != "Excel")
            return BadRequest("Sync is available only for 'Excel' connectors");

        var employees = dto.Employees.Adapt<IEnumerable<Employee>>();

        var syncContext = new SyncContext
        {
            ConnectorId = connectorId,
            ConnectorType = connector.Type,
            NetworkId = connector.NetworkId,
            TimeRange = new TimeRange(nextSyncStart, clock.UtcNow()),
            AccessToken = accessToken,
            NetworkProperties = connector.Properties,
            Employees = employees
        };

        await workerRouter.StartSyncAsync(connector.Worker.Name, syncContext);

        return Accepted();
    }
}