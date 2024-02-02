using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;

namespace NetworkPerspective.Sync.Excel.Controllers;

[Route("sync")]
public class SyncController : ApiControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ISyncContextFactory _syncContextFactory;


    public SyncController(INetworkPerspectiveCore networkPerspectiveCore, ISyncService syncService, ISyncContextFactory syncContextFactory, INetworkIdInitializer networkIdInitializer)
        : base(networkPerspectiveCore, networkIdInitializer)
    {
        _syncService = syncService;
        _syncContextFactory = syncContextFactory;
    }

    [HttpPost]
    public async Task<IActionResult> SyncAsync(SyncRequestDto syncRequest, CancellationToken stoppingToken = default)
    {
        // validate token
        var tokenValidationResult = await ValidateTokenAsync(stoppingToken);

        // create sync context
        using var syncContext = await _syncContextFactory.CreateForNetworkAsync(tokenValidationResult.NetworkId, stoppingToken);

        // add employees & metadata to sync context
        syncContext.Set(syncRequest.Employees);

        // create sync service & sync data
        await _syncService.SyncAsync(syncContext, stoppingToken);

        // return success or throw exception
        return Ok();
    }
}