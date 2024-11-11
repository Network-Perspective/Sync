using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;
using NetworkPerspective.Sync.Orchestrator.Persistence.Mappers;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Repositories;

internal class ConnectorRepository(DbSet<ConnectorEntity> dbSet) : IConnectorRepository
{
    public async Task AddAsync(Connector connector, CancellationToken stoppingToken = default)
    {
        try
        {
            var entity = ConnectorMapper.DomainModelToEntity(connector);
            await dbSet.AddAsync(entity, stoppingToken);
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
            var entity = await dbSet.SingleAsync(x => x.Id == networkId, stoppingToken);
            dbSet.Remove(entity);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<Connector> FindAsync(Guid id, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await dbSet
                .Include(x => x.Properties)
                .Include(x => x.Worker)
                .SingleOrDefaultAsync(x => x.Id == id, stoppingToken);

            return result is null ? null : ConnectorMapper.EntityToDomainModel(result);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<Connector> GetAsync(Guid id, CancellationToken stoppingToken = default)
    {
        if (!await ExistsAsync(id, stoppingToken))
            throw new EntityNotFoundException<Connector>();

        try
        {
            var result = await dbSet
                .Include(x => x.Properties)
                .Include(x => x.Worker)
                .SingleAsync(x => x.Id == id, stoppingToken);

            return ConnectorMapper.EntityToDomainModel(result);
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
            var entities = await dbSet
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
            var entities = await dbSet
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

    private async Task<bool> ExistsAsync(Guid id, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await dbSet
                .SingleOrDefaultAsync(x => x.Id == id, stoppingToken);

            return result is not null;
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }
}