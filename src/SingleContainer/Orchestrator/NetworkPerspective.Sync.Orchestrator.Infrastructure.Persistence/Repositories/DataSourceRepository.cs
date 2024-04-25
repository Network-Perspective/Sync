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

internal class DataSourceRepository<TProperties> : IDataSourceRepository<TProperties>
    where TProperties : DataSourceProperties, new()
{
    private readonly DbSet<DataSourceEntity> _dbSet;

    public DataSourceRepository(DbSet<DataSourceEntity> dbSet)
    {
        _dbSet = dbSet;
    }

    public async Task AddAsync(DataSource<TProperties> dataSource, CancellationToken stoppingToken = default)
    {
        try
        {
            var entity = DataSourceMapper<TProperties>.DomainModelToEntity(dataSource);
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

    public async Task<DataSource<TProperties>> FindAsync(Guid networkId, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await _dbSet
                .Include(x => x.Properties)
                .Where(x => x.Id == networkId)
                .FirstOrDefaultAsync(stoppingToken);

            return result is null ? null : DataSourceMapper<TProperties>.EntityToDomainModel(result);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<IEnumerable<DataSource<TProperties>>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            var entities = await _dbSet
                .Include(x => x.Properties)
                .ToListAsync(stoppingToken);

            return entities.Select(DataSourceMapper<TProperties>.EntityToDomainModel);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }
}