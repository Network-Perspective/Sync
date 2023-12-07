using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Framework.Controllers;

using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Excel.Dtos;
using NetworkPerspective.Sync.Infrastructure.Excel.Services;

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
    public async Task<IActionResult> SyncAsync(List<EmployeeDto> employees, CancellationToken stoppingToken = default)
    {
        // validate token
        var tokenValidationResult = await ValidateTokenAsync(stoppingToken);

        // create sync context
        using var syncContext =
            await _syncContextFactory.CreateForNetworkAsync(tokenValidationResult.NetworkId, stoppingToken);
        
        // add employees to sync context
        syncContext.Set(employees);
        
        // create sync service & sync
        var syncService = await _syncServiceFactory.CreateAsync(syncContext.NetworkId, stoppingToken);
        await syncService.SyncAsync(syncContext, stoppingToken);

        // return success or throw exception
        return Ok();
    }
}