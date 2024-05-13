using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;
using NetworkPerspective.Sync.Orchestrator.Dtos;
using NetworkPerspective.Sync.Orchestrator.Mappers;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route("api/connectors")]
[Authorize(AuthenticationSchemes = ApiKeyAuthOptions.DefaultScheme)]
public class ConnectorsController : ControllerBase
{
    private readonly IConnectorsService _connectorsService;
    private readonly ILogger<ConnectorsController> _logger;

    public ConnectorsController(IConnectorsService connectorsService, ILogger<ConnectorsController> logger)
    {
        _connectorsService = connectorsService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateConnectorDto request, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to create new connector '{type}' for worker '{workerId}'", request.Type, request.WorkerId);

        var properties = request.Properties.ToDictionary(p => p.Key, p => p.Value);
        await _connectorsService.CreateAsync(request.Type, request.WorkerId, properties, stoppingToken);

        return Ok();
    }

    [HttpGet]
    public async Task<IEnumerable<ConnectorDto>> GetAllAsync([FromQuery] Guid workerId, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to get all connectors of worker '{workerId}'", workerId);

        var workers = await _connectorsService.GetAllAsync(workerId, stoppingToken);
        return workers.Select(ConnectorMapper.ToDto);
    }
}