using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IWorkersService
{
    Task CreateAsync(Guid id, CancellationToken stoppingToken = default);
}

internal class WorkersService : IWorkersService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<WorkersService> _logger;

    public WorkersService(IUnitOfWork unitOfWork, IClock clock, ILogger<WorkersService> logger)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task CreateAsync(Guid id, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Creating new worker '{id}'...", id);

        var worker = new Worker(id, _clock.UtcNow());

        await _unitOfWork
            .GetWorkerRepository()
            .AddAsync(worker, stoppingToken);

        await _unitOfWork.CommitAsync(stoppingToken);

        _logger.LogInformation("New worker '{id}' has been created", id);


        throw new NotImplementedException();
    }
}