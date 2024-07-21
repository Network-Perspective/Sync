using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Mapster;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;
using NetworkPerspective.Sync.Orchestrator.Dtos;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route("api/connectors")]
[Authorize(AuthenticationSchemes = ApiKeyAuthOptions.DefaultScheme)]
public class ConnectorsController : ControllerBase
{
    private readonly IConnectorsService _connectorsService;
    private readonly ISyncScheduler _syncScheduler;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ConnectorsController> _logger;

    public ConnectorsController(IConnectorsService connectorsService, ISyncScheduler syncScheduler, ITokenService tokenService, ILogger<ConnectorsController> logger)
    {
        _connectorsService = connectorsService;
        _syncScheduler = syncScheduler;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateConnectorDto request, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to create new connector '{type}' for worker '{workerId}'", request.Type, request.WorkerId);

        var properties = request.Properties.ToDictionary(p => p.Key, p => p.Value);
        await _connectorsService.CreateAsync(request.Id, request.NetworkId, request.Type, request.WorkerId, properties, stoppingToken);

        await _tokenService.AddOrReplace(request.AccessToken.ToSecureString(), request.Id, stoppingToken);
        await _syncScheduler.AddOrReplaceAsync(request.Id, stoppingToken);

        return Ok();
    }

    [HttpGet]
    public async Task<IEnumerable<ConnectorDto>> GetAllAsync([FromQuery] Guid workerId, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to get all connectors of worker '{workerId}'", workerId);

        var workers = await _connectorsService.GetAllOfWorkerAsync(workerId, stoppingToken);
        return workers.Adapt<IEnumerable<ConnectorDto>>();
    }
}