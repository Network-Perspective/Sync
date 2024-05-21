using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Framework.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Application.Extensions;
using System;

namespace NetworkPerspective.Sync.Framework.Controllers;

[Route("api/connectors/{connectorId:guid}/schedules")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly ISyncScheduler _scheduler;
    private readonly IConnectorsService _connectorsService;
    private readonly ISyncHistoryService _syncHistoryService;
    private readonly IStatusLogger _statusLogger;

    public SchedulesController(ISyncScheduler scheduler, IConnectorsService connectorsService, ISyncHistoryService syncHistoryService, IStatusLogger statusLogger)
    {
        _scheduler = scheduler;
        _connectorsService = connectorsService;
        _syncHistoryService = syncHistoryService;
        _statusLogger = statusLogger;
    }

    /// <summary>
    /// Schedules synchronization job for given network to run at midnight, and also triggers the synchronization to run now
    /// </summary>
    /// <param name="connectorId">Connector Id</param>
    /// <param name="request">Scheduler properties</param>
    /// <param name="stoppingToken">Stopping token</param>
    /// <response code="200">Synchronization scheduled</response>
    /// <response code="401">Missing or invalid authorization token</response>
    /// <response code="404">Network doesn't exist</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartAsync([FromRoute]Guid connectorId, [FromBody] SchedulerStartDto request, CancellationToken stoppingToken = default)
    {
        await _connectorsService.ValidateExists(connectorId, stoppingToken);

        if (request.OverrideSyncPeriodStart is not null)
            await _syncHistoryService.OverrideSyncStartAsync(connectorId, request.OverrideSyncPeriodStart.Value.ToUniversalTime(), stoppingToken);

        await _scheduler.ScheduleAsync(connectorId, stoppingToken);
        await _scheduler.TriggerNowAsync(connectorId, stoppingToken);

        await _statusLogger
            .LogInfoAsync(connectorId, "Schedule started", stoppingToken);

        return Ok($"Scheduled sync {connectorId}");
    }

    /// <summary>
    /// Unschedules synchronization job for given network
    /// </summary>
    /// <param name="connectorId">Connector Id</param>
    /// <param name="stoppingToken">Stopping token</param>
    /// <response code="200">Synchronization scheduled</response>
    /// <response code="401">Missing or invalid authorization token</response>
    /// <response code="404">Network doesn't exist</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StopAsync([FromRoute] Guid connectorId, CancellationToken stoppingToken = default)
    {
        await _connectorsService.ValidateExists(connectorId, stoppingToken);

        await _scheduler.UnscheduleAsync(connectorId, stoppingToken);
        await _scheduler.InterruptNowAsync(connectorId, stoppingToken);

        await _statusLogger
            .LogInfoAsync(connectorId, "Schedule stopped", stoppingToken);

        return Ok($"Unscheduled sync {connectorId}");
    }
}