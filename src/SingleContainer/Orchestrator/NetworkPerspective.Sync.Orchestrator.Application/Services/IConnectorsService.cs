using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IConnectorsService
{
    Task CreateAsync(Guid id, Guid networkId, string type, Guid workerId, IDictionary<string, string> properties, CancellationToken stoppingToken = default);
    Task RemoveAsync(Guid id, CancellationToken stoppingToken = default);
    Task<Connector> GetAsync(Guid id, CancellationToken stoppingToken = default);
    Task<IEnumerable<Connector>> GetAllOfWorkerAsync(Guid workerId, CancellationToken stoppingToken = default);
    Task<IEnumerable<Connector>> GetAllAsync(CancellationToken stoppingToken = default);
    Task ValidateExists(Guid id, CancellationToken stoppingToken = default);
}

internal class ConnectorsService : IConnectorsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<ConnectorsService> _logger;

    public ConnectorsService(IUnitOfWork unitOfWork, IClock clock, ILogger<ConnectorsService> logger)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task CreateAsync(Guid id, Guid networkId, string type, Guid workerId, IDictionary<string, string> properties, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Creating new connector '{id}' of '{type}'...", id, type);

        var worker = await _unitOfWork
            .GetWorkerRepository()
            .GetAsync(workerId, stoppingToken);

        var now = _clock.UtcNow();
        var connector = new Connector(id, type, properties, worker, networkId, now);

        await _unitOfWork
            .GetConnectorRepository()
            .AddAsync(connector, stoppingToken);

        await _unitOfWork.CommitAsync(stoppingToken);

        _logger.LogInformation("New connector '{id}' has been created", id);
    }

    public async Task RemoveAsync(Guid id, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Deleting new connector '{id}'", id);

        await ValidateExists(id, stoppingToken);

        await _unitOfWork
            .GetConnectorRepository()
            .RemoveAsync(id, stoppingToken);

        await _unitOfWork.CommitAsync(stoppingToken);

        _logger.LogInformation("New connector '{id}' has been removed", id);
    }

    public async Task<IEnumerable<Connector>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting all connectors...");

        return await _unitOfWork
            .GetConnectorRepository()
            .GetAllAsync(stoppingToken);
    }

    public async Task<IEnumerable<Connector>> GetAllOfWorkerAsync(Guid workerId, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting all connectors of worker '{workerId}'...", workerId);

        return await _unitOfWork
            .GetConnectorRepository()
            .GetAllOfWorkerAsync(workerId, stoppingToken);
    }

    public async Task<Connector> GetAsync(Guid id, CancellationToken stoppingToken = default)
    {
        var connector = await _unitOfWork
            .GetConnectorRepository()
            .FindAsync(id, stoppingToken);

        if (connector is null)
            throw new ConnectorNotFoundException(id);

        return connector;
    }

    public async Task ValidateExists(Guid id, CancellationToken stoppingToken = default)
    {
        var connector = await _unitOfWork
            .GetConnectorRepository()
            .FindAsync(id, stoppingToken);

        if (connector is null)
            throw new ConnectorNotFoundException(id);
    }
}