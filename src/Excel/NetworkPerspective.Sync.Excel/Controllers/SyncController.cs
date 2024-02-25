using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
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

    [HttpPost]
    public async Task<IActionResult> SyncAsync(SyncRequestDto syncRequest, CancellationToken stoppingToken = default)
    {
        // create sync context
        using var syncContext = await _syncContextFactory.CreateForNetworkAsync(_networkIdProvider.Get(), stoppingToken);

        // add employees & metadata to sync context
        syncContext.Set(syncRequest.Employees);

        // create sync service & sync data
        await _syncService.SyncAsync(syncContext, stoppingToken);

        // return success or throw exception
        return Ok();
    }
}