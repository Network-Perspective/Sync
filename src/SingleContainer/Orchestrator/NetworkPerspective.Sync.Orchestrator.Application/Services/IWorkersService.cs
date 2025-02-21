using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IWorkersService
{
    Task<Worker> GetAsync(Guid id, CancellationToken stoppingToken = default);
    Task<Worker> GetAsync(string name, CancellationToken stoppingToken = default);
    Task<IEnumerable<Worker>> GetAllAsync(CancellationToken stoppingToken = default);
    Task CreateAsync(Guid id, string name, string secret, CancellationToken stoppingToken = default);
    Task AuthorizeAsync(Guid id, CancellationToken stoppingToken = default);
    Task<Worker> AuthenticateAsync(string name, string password, CancellationToken stoppingToken = default);
    Task EnsureRemoved(Guid id, CancellationToken stoppingToken = default);
}

internal class WorkersService(IUnitOfWork unitOfWork, IWorkerRouter workerRouter, IClock clock, ICryptoService cryptoService, ILogger<WorkersService> logger) : IWorkersService
{
    private readonly ILogger<IWorkersService> _logger = logger;

    public async Task CreateAsync(Guid id, string name, string secret, CancellationToken stoppingToken = default)
    {
        const int workerProtocolVersion = 1;

        _logger.LogInformation("Creating new worker '{name}'...", name.Sanitize());

        var keySalt = cryptoService.GenerateSalt();
        var hashedSecret = cryptoService.HashPassword(secret, keySalt);
        var keySaltBase64 = Convert.ToBase64String(keySalt);
        var hashedSecretBase64 = Convert.ToBase64String(hashedSecret);
        var now = clock.UtcNow();

        var worker = new Worker(id, workerProtocolVersion, name, hashedSecretBase64, keySaltBase64, false, now);

        await unitOfWork
            .GetWorkerRepository()
            .AddAsync(worker, stoppingToken);

        await unitOfWork.CommitAsync(stoppingToken);

        _logger.LogInformation("New worker '{name}' has been created", name.Sanitize());
    }

    public async Task AuthorizeAsync(Guid id, CancellationToken stoppingToken = default)
    {
        var repo = unitOfWork
            .GetWorkerRepository();

        var worker = await repo.GetAsync(id, stoppingToken);
        worker.Authorize();
        await repo.UpdateAsync(worker, stoppingToken);

        await unitOfWork.CommitAsync(stoppingToken);
    }

    public async Task<Worker> AuthenticateAsync(string name, string password, CancellationToken stoppingToken = default)
    {
        var worker = await unitOfWork
            .GetWorkerRepository()
            .GetAsync(name, stoppingToken);

        var isPasswordValid = cryptoService.VerifyPassword(password, worker.SecretHash, worker.SecretSalt);

        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        if (!worker.IsAuthorized)
            throw new WorkerNotAuthorizedException(name);

        return worker;
    }

    public async Task EnsureRemoved(Guid id, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Deleting worker '{id}'...", id);

        var repository = unitOfWork.GetWorkerRepository();
        var exists = await repository.ExistsAsync(id, stoppingToken);

        if (exists)
        {
            await repository.RemoveAsync(id, stoppingToken);
            await unitOfWork.CommitAsync(stoppingToken);
            _logger.LogDebug("Worker '{id}' has been removed", id);
        }

        _logger.LogInformation("Worker '{id}' does not exist, nothing to delete", id);
    }

    public async Task<IEnumerable<Worker>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        var workers = await unitOfWork
            .GetWorkerRepository()
            .GetAllAsync(stoppingToken);

        var newWorkers = new List<Worker>();

        foreach (var worker in workers)
            newWorkers.Add(await SetConnectionPropertiesAsync(worker));

        return newWorkers;
    }

    public async Task<Worker> GetAsync(Guid id, CancellationToken stoppingToken = default)
    {
        var worker = await unitOfWork
            .GetWorkerRepository()
            .GetAsync(id, stoppingToken);

        await SetConnectionPropertiesAsync(worker);

        return worker;
    }

    public async Task<Worker> GetAsync(string name, CancellationToken stoppingToken = default)
    {
        var worker = await unitOfWork
            .GetWorkerRepository()
            .GetAsync(name, stoppingToken);

        worker = await SetConnectionPropertiesAsync(worker);

        return worker;
    }

    private async Task<Worker> SetConnectionPropertiesAsync(Worker worker)
    {
        var isOnline = workerRouter.IsConnected(worker.Name);
        worker.SetOnlineStatus(isOnline);

        if (isOnline)
        {
            var supportedConnectorTypes = await workerRouter.GetSupportedConnectorTypesAsync(worker.Name);
            worker.SetSupportedConnectorTypes(supportedConnectorTypes);
        }

        return worker;
    }
}