using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IConnectorsService
{
    Task CreateAsync(Guid id, Guid networkId, string type, Guid workerId, IDictionary<string, string> properties, CancellationToken stoppingToken = default);
    Task RemoveAsync(Guid id, CancellationToken stoppingToken = default);
    Task<Connector> GetAsync(Guid id, CancellationToken stoppingToken = default);
    Task<IEnumerable<Connector>> GetAllOfWorkerAsync(Guid workerId, CancellationToken stoppingToken = default);
    Task<IEnumerable<Connector>> GetAllAsync(CancellationToken stoppingToken = default);
    Task UpdatePropertiesAsync(Guid id, IDictionary<string, string> properties, CancellationToken stoppingToken = default);
    Task ValidateExists(Guid id, CancellationToken stoppingToken = default);
}

internal class ConnectorsService(IUnitOfWork unitOfWork, IClock clock, ILogger<ConnectorsService> logger) : IConnectorsService
{
    public async Task CreateAsync(Guid id, Guid networkId, string type, Guid workerId, IDictionary<string, string> properties, CancellationToken stoppingToken = default)
    {
        // codeql [suppress] cs/log-forging: User input is validated and sanitized
        logger.LogInformation("Creating new connector '{id}' of '{type}'...", id, type.Sanitize());

        var worker = await unitOfWork
            .GetWorkerRepository()
            .GetAsync(workerId, stoppingToken);

        var now = clock.UtcNow();
        var connector = new Connector(id, type, properties, worker, networkId, now);

        await unitOfWork
            .GetConnectorRepository()
            .AddAsync(connector, stoppingToken);

        await unitOfWork.CommitAsync(stoppingToken);

        logger.LogInformation("New connector '{id}' has been created", id);
    }

    public async Task RemoveAsync(Guid id, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Deleting new connector '{id}'", id);

        await ValidateExists(id, stoppingToken);

        await unitOfWork
            .GetConnectorRepository()
            .RemoveAsync(id, stoppingToken);

        await unitOfWork.CommitAsync(stoppingToken);

        logger.LogInformation("New connector '{id}' has been removed", id);
    }

    public async Task<IEnumerable<Connector>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Getting all connectors...");

        return await unitOfWork
            .GetConnectorRepository()
            .GetAllAsync(stoppingToken);
    }

    public async Task<IEnumerable<Connector>> GetAllOfWorkerAsync(Guid workerId, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Getting all connectors of worker '{workerId}'...", workerId);

        return await unitOfWork
            .GetConnectorRepository()
            .GetAllOfWorkerAsync(workerId, stoppingToken);
    }

    public async Task<Connector> GetAsync(Guid id, CancellationToken stoppingToken = default)
    {
        var connector = await unitOfWork
            .GetConnectorRepository()
            .FindAsync(id, stoppingToken);

        if (connector is null)
            throw new ConnectorNotFoundException(id);

        return connector;
    }

    public async Task UpdatePropertiesAsync(Guid id, IDictionary<string, string> properties, CancellationToken stoppingToken = default)
    {
        await ValidateExists(id, stoppingToken);

        await unitOfWork.GetConnectorPropertyRepository()
            .SetAsync(id, properties, stoppingToken);

        await unitOfWork.CommitAsync(stoppingToken);
    }

    public async Task ValidateExists(Guid id, CancellationToken stoppingToken = default)
    {
        var connector = await unitOfWork
            .GetConnectorRepository()
            .FindAsync(id, stoppingToken);

        if (connector is null)
            throw new ConnectorNotFoundException(id);
    }
}