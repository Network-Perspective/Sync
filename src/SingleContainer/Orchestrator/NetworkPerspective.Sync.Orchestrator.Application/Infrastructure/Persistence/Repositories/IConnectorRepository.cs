using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

public interface IConnectorRepository
{
    Task AddAsync(Connector connector, CancellationToken stoppingToken = default);
    Task RemoveAsync(Guid connectorId, CancellationToken stoppingToken = default);
    Task<Connector> FindAsync(Guid connectorId, CancellationToken stoppingToken = default);
    Task<IEnumerable<Connector>> GetAllAsync(CancellationToken stoppingToken = default);
}