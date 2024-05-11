using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IWorkersService
{
    Task<IEnumerable<Worker>> GetAllAsync(CancellationToken stoppingToken = default);
    Task CreateAsync(string name, string secret, CancellationToken stoppingToken = default);
    Task AuthorizeAsync(Guid id, CancellationToken stoppingToken = default);
    Task EnsureRemoved(Guid id, CancellationToken stoppingToken = default);
}

internal class WorkersService : IWorkersService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICryptoService _cryptoService;
    private readonly ILogger<WorkersService> _logger;

    public WorkersService(IUnitOfWork unitOfWork, IClock clock, ICryptoService cryptoService, ILogger<WorkersService> logger)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _cryptoService = cryptoService;
        _logger = logger;
    }

    public async Task CreateAsync(string name, string secret, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Creating new worker '{name}'...", name);

        var id = Guid.NewGuid();
        var keySalt = _cryptoService.GenerateSalt();
        var hashedSecret = _cryptoService.HashPassword(secret, keySalt);
        var keySaltBase64 = Convert.ToBase64String(keySalt);
        var hashedSecretBase64 = Convert.ToBase64String(hashedSecret);
        var now = _clock.UtcNow();

        var worker = new Worker(id, name, hashedSecretBase64, keySaltBase64, false, now);

        await _unitOfWork
            .GetWorkerRepository()
            .AddAsync(worker, stoppingToken);

        await _unitOfWork.CommitAsync(stoppingToken);

        _logger.LogInformation("New worker '{id}' has been created", id);
    }

    public async Task AuthorizeAsync(Guid id, CancellationToken stoppingToken = default)
    {
        var repo = _unitOfWork
            .GetWorkerRepository();

        var worker = await repo.GetAsync(id, stoppingToken);
        worker.Authorize();
        await repo.UpdateAsync(worker, stoppingToken);

        await _unitOfWork.CommitAsync(stoppingToken);
    }

    public async Task EnsureRemoved(Guid id, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Deleting worker '{id}'...", id);

        var repository = _unitOfWork.GetWorkerRepository();
        var exists = await repository.ExistsAsync(id, stoppingToken);

        if (exists)
        {
            await repository.RemoveAsync(id, stoppingToken);
            await _unitOfWork.CommitAsync(stoppingToken);
            _logger.LogDebug("Worker '{id}' has been removed", id);
        }

        _logger.LogInformation("Worker '{id}' does not exist, nothing to delete", id);
    }

    public Task<IEnumerable<Worker>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        return _unitOfWork
            .GetWorkerRepository()
            .GetAllAsync(stoppingToken);
    }
}