using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories
{
    public interface IConnectorRepository<TProperties> where TProperties : ConnectorProperties, new()
    {
        Task AddAsync(Connector<TProperties> network, CancellationToken stoppingToken = default);
        Task RemoveAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task<Connector<TProperties>> FindAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task<IEnumerable<Connector<TProperties>>> GetAllAsync(CancellationToken stoppingToken = default);
    }
}