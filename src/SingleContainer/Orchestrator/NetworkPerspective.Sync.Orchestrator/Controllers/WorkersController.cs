using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;
using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route("api/workers")]
[Authorize(AuthenticationSchemes = ApiKeyAuthOptions.DefaultScheme)]
public class WorkersController(IValidator<CreateWorkerDto> validator, IWorkersService workersService, ILogger<WorkersController> logger) : ControllerBase
{
    [HttpGet("{name}")]
    public async Task<WorkerDto> GetAsync(string name, CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Received request to get worker '{Name}'", name.Sanitize());
        var worker = await workersService.GetAsync(name, stoppingToken);
        return worker.Adapt<WorkerDto>();
    }

    [HttpGet]
    public async Task<IEnumerable<WorkerDto>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Received request to get all workers");
        var workers = await workersService.GetAllAsync(stoppingToken);
        var result = workers.Adapt<IEnumerable<WorkerDto>>();
        return result;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWorkerDto request, CancellationToken stoppingToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, stoppingToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        logger.LogDebug("Received request to create new worker '{Name}'", request.Name.Sanitize());
        await workersService.CreateAsync(request.Id, request.Name, request.Secret, stoppingToken);

        return Ok();
    }


    [HttpPost("{id:guid}/auth")]
    public async Task<IActionResult> AuthorizeAsync(Guid id, CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Received request to authorize worker '{id}'", id);
        await workersService.AuthorizeAsync(id, stoppingToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveAsync(Guid id, CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Received request to delete worker '{id}'", id);
        await workersService.EnsureRemoved(id, stoppingToken);

        return Ok();
    }
}