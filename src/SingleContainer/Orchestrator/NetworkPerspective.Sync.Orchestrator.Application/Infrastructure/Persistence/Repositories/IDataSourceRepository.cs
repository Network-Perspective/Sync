using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

public interface IDataSourceRepository<TProperties> where TProperties : DataSourceProperties, new()
{
    Task AddAsync(DataSource<TProperties> network, CancellationToken stoppingToken = default);
    Task RemoveAsync(Guid networkId, CancellationToken stoppingToken = default);
    Task<DataSource<TProperties>> FindAsync(Guid networkId, CancellationToken stoppingToken = default);
    Task<IEnumerable<DataSource<TProperties>>> GetAllAsync(CancellationToken stoppingToken = default);
}