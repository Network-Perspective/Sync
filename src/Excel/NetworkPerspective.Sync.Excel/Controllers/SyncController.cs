using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Services;

using Swashbuckle.AspNetCore.Annotations;

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

    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, "Network added", typeof(string))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or validation error")]
    public async Task<IActionResult> SyncAsync(SyncRequestDto syncRequest, CancellationToken stoppingToken = default)
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