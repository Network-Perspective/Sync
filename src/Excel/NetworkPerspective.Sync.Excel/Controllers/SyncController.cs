using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Infrastructure.Excel.Dtos;

namespace NetworkPerspective.Sync.Excel.Controllers;

[Route("sync")]
public class SyncController : ApiControllerBase
{
    private readonly ISyncServiceFactory _syncServiceFactory;
    private readonly ISyncContextFactory _syncContextFactory;


    public SyncController(INetworkPerspectiveCore networkPerspectiveCore, ISyncServiceFactory syncServiceFactory,
        ISyncContextFactory syncContextFactory) : base(networkPerspectiveCore)
    {
        _syncServiceFactory = syncServiceFactory;
        _syncContextFactory = syncContextFactory;
    }

    [HttpPost]
    public async Task<IActionResult> SyncAsync(List<EmployeeDto> employees, SyncMetadataDto metadata, CancellationToken stoppingToken = default)
    {
        // validate token
        var tokenValidationResult = await ValidateTokenAsync(stoppingToken);

        // create sync context
        using var syncContext =
            await _syncContextFactory.CreateForNetworkAsync(tokenValidationResult.NetworkId, stoppingToken);

        // add employees & metadata to sync context
        syncContext.Set(employees);
        syncContext.Set(metadata);

        // create sync service & sync data
        var syncService = await _syncServiceFactory.CreateAsync(syncContext.NetworkId, stoppingToken);
        await syncService.SyncAsync(syncContext, stoppingToken);

        // return success or throw exception
        return Ok();
    }
}