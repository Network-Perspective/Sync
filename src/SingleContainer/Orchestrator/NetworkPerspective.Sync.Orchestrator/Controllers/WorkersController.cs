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

[Route("api/workers")]
[Authorize(AuthenticationSchemes = ApiKeyAuthOptions.DefaultScheme)]
public class WorkersController : ControllerBase
{
    private readonly IWorkersService _workersService;
    private readonly ILogger<WorkersController> _logger;

    public WorkersController(IWorkersService workersService, ILogger<WorkersController> logger)
    {
        _workersService = workersService;
        _logger = logger;
    }

    [HttpGet("{name}")]
    public async Task<WorkerDto> GetAsync(string name, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to get all workers");
        var worker = await _workersService.GetAsync(name, stoppingToken);
        return WorkerMapper.ToDto(worker);
    }

    [HttpGet]
    public async Task<IEnumerable<WorkerDto>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to get all workers");
        var workers = await _workersService.GetAllAsync(stoppingToken);
        return workers.Select(WorkerMapper.ToDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWorkerDto request, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to create new worker '{name}'", request.Name);
        await _workersService.CreateAsync(request.Name, request.Secret, stoppingToken);

        return Ok();
    }


    [HttpPost("{id:guid}/auth")]
    public async Task<IActionResult> AuthorizeAsync(Guid id, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to authorize worker '{id}'", id);
        await _workersService.AuthorizeAsync(id, stoppingToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveAsync(Guid id, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Received request to delete worker '{id}'", id);
        await _workersService.EnsureRemoved(id, stoppingToken);

        return Ok();
    }
}