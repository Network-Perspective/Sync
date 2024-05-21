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
    Task<Guid> CreateAsync(string type, Guid workerId, IDictionary<string, string> properties, CancellationToken stoppingToken = default);
    Task<Connector> GetAsync(Guid id, CancellationToken stoppingToken);
    Task<IEnumerable<Connector>> GetAllAsync(Guid workerId, CancellationToken stoppingToken);
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

    public async Task<Guid> CreateAsync(string type, Guid workerId, IDictionary<string, string> properties, CancellationToken stoppingToken = default)
    {
        var id = Guid.NewGuid();
        _logger.LogInformation("Creating new connector '{id}' of '{type}'...", id, type);

        var worker = await _unitOfWork
            .GetWorkerRepository()
            .GetAsync(workerId, stoppingToken);

        var networkId = Guid.NewGuid();
        var now = _clock.UtcNow();
        var connector = new Connector(id, type, properties, worker, networkId, now);

        await _unitOfWork
            .GetConnectorRepository()
            .AddAsync(connector, stoppingToken);

        await _unitOfWork.CommitAsync(stoppingToken);

        _logger.LogInformation("New connector '{id}' has been created", id);
        return id;
    }

    public async Task<IEnumerable<Connector>> GetAllAsync(Guid workerId, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Getting all connectors of worker '{workerId}'...", workerId);

        return await _unitOfWork
            .GetConnectorRepository()
            .GetAllAsync(workerId, stoppingToken);
    }

    public async Task<Connector> GetAsync(Guid id, CancellationToken stoppingToken)
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