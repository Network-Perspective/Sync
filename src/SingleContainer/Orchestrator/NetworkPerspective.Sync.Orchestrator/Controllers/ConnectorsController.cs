using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    /// <summary>
    /// Creates new connector for selected worker instance with specified properties
    /// </summary>
    /// <param name="request">Connector parameters</param>
    /// <param name="stoppingToken">Stopping Token</param>
    /// <response code="200">Connector created</response>
    /// <response code="401">Missing or invalid authorization token</response>
    /// <response code="404">Worker doesnt exist</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateConnectorDto request, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to create new connector '{type}' for worker '{workerId}'", request.Type, request.WorkerId);

        var properties = request.Properties.ToDictionary(p => p.Key, p => p.Value);
        await _connectorsService.CreateAsync(request.Id, request.NetworkId, request.Type, request.WorkerId, properties, stoppingToken);

        await _tokenService.AddOrReplace(request.AccessToken.ToSecureString(), request.Id, stoppingToken);
        await _syncScheduler.AddOrReplaceAsync(request.Id, stoppingToken);

        return Ok();
    }

    /// <summary>
    /// Get list of connectors of given worker
    /// </summary>
    /// <param name="workerId">Worker Id</param>
    /// <param name="stoppingToken">Stopping token</param>
    /// <returns>List of connectors of given worker</returns>
    /// <response code="200">Ok</response>
    /// <response code="401">Missing or invalid authorization token</response>
    /// <response code="404">Worker doesnt exist</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ConnectorDto>> GetAllAsync([FromQuery] Guid workerId, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to get all connectors of worker '{workerId}'", workerId);

        var workers = await _connectorsService.GetAllOfWorkerAsync(workerId, stoppingToken);
        return workers.Adapt<IEnumerable<ConnectorDto>>();
    }
}