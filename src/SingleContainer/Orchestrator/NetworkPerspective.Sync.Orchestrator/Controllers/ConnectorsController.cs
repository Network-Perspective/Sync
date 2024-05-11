using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route("api/connectors")]
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
        _logger.LogDebug("Received request to create new connector '{id}' for worker '{workerId}'", request.Id, request.WorkerId);

        var properties = request.Properties.ToDictionary(p => p.Key, p => p.Value);
        await _connectorsService.CreateAsync(request.Id, request.Type, request.WorkerId, properties, stoppingToken);

        return Created();
    }
}