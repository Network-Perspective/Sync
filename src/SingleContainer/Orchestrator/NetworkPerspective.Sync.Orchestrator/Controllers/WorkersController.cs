using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[AllowAnonymous]
[Route("api/workers")]
public class WorkersController : ControllerBase
{
    private readonly IWorkersService _workersService;
    private readonly ILogger<WorkersController> _logger;

    public WorkersController(IWorkersService workersService, ILogger<WorkersController> logger)
    {
        _workersService = workersService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWorkerDto request, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to create new worker '{id}'", request.Id);
        await _workersService.CreateAsync(request.Id, stoppingToken);

        return Created();
    }
}