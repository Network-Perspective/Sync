using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Excel.Controllers;

[Route("sync")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ISyncContextFactory _syncContextFactory;
    private readonly INetworkIdProvider _networkIdProvider;

    public SyncController(ISyncService syncService, ISyncContextFactory syncContextFactory, INetworkIdProvider networkIdProvider)
    {
        _syncService = syncService;
        _syncContextFactory = syncContextFactory;
        _networkIdProvider = networkIdProvider;
    }

    /// <summary>
    /// Sync
    /// </summary>
    /// <param name="syncRequest"></param>
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
    public async Task<IActionResult> SyncAsync([FromBody] SyncRequestDto syncRequest, CancellationToken stoppingToken = default)
    {
        // create sync context
        using var syncContext = await _syncContextFactory.CreateForNetworkAsync(_networkIdProvider.Get(), stoppingToken);

        // add employees & metadata to sync context
        syncContext.Set(syncRequest.Employees);

        // create sync service & sync data
        try
        {
            await _syncService.SyncAsync(syncContext, stoppingToken);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }

        // return success or throw exception
        return Ok();
    }
}