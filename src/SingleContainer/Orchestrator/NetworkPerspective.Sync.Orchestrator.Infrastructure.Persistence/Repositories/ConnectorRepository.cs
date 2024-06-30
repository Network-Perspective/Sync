using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Mappers;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Repositories;

internal class ConnectorRepository : IConnectorRepository
{
    private readonly DbSet<ConnectorEntity> _dbSet;

    public ConnectorRepository(DbSet<ConnectorEntity> dbSet)
    {
        _dbSet = dbSet;
    }

    public async Task AddAsync(Connector connector, CancellationToken stoppingToken = default)
    {
        try
        {
            var entity = ConnectorMapper.DomainModelToEntity(connector);
            await _dbSet.AddAsync(entity, stoppingToken);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task RemoveAsync(Guid networkId, CancellationToken stoppingToken = default)
    {
        try
        {
            var entity = await _dbSet.SingleAsync(x => x.Id == networkId, stoppingToken);
            _dbSet.Remove(entity);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<Connector> FindAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await _dbSet
                .Include(x => x.Properties)
                .Include(x => x.Worker)
                .Where(x => x.Id == connectorId)
                .FirstOrDefaultAsync(stoppingToken);

            return result is null ? null : ConnectorMapper.EntityToDomainModel(result);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<IEnumerable<Connector>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            var entities = await _dbSet
                .Include(x => x.Properties)
                .Include(x => x.Worker)
                .ToListAsync(stoppingToken);

            return entities.Select(ConnectorMapper.EntityToDomainModel);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<IEnumerable<Connector>> GetAllOfWorkerAsync(Guid workerId, CancellationToken stoppingToken = default)
    {
        try
        {
            var entities = await _dbSet
                .Where(x => x.WorkerId == workerId)
                .Include(x => x.Properties)
                .Include(x => x.Worker)
                .ToListAsync(stoppingToken);

            return entities.Select(ConnectorMapper.EntityToDomainModel);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }
}