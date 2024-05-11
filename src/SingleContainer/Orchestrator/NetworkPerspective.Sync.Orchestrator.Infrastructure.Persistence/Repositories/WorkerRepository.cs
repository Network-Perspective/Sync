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

internal class WorkerRepository : IWorkerRepository
{
    private readonly DbSet<WorkerEntity> _dbSet;

    public WorkerRepository(DbSet<WorkerEntity> dbSet)
    {
        _dbSet = dbSet;
    }

    public async Task AddAsync(Worker worker, CancellationToken stoppingToken = default)
    {
        try
        {
            var entity = WorkerMapper.DomainModelToEntity(worker);
            await _dbSet.AddAsync(entity, stoppingToken);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await _dbSet
                .FirstOrDefaultAsync(x => x.Id == id, stoppingToken);

            return result is not null;
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<IEnumerable<Worker>> GetAllAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await _dbSet.ToListAsync(stoppingToken);

            return result.Select(WorkerMapper.EntityToDomainModel);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<Worker> GetAsync(Guid id, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await _dbSet
                .FirstOrDefaultAsync(x => x.Id == id, stoppingToken);

            if (result is null)
                throw new EntityNotFoundException(typeof(Worker));

            return WorkerMapper.EntityToDomainModel(result);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await _dbSet.SingleAsync(x => x.Id == id, stoppingToken);
            _dbSet.Remove(result);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task UpdateAsync(Worker worker, CancellationToken stoppingToken = default)
    {
        try
        {
            var current = await _dbSet
                .FirstOrDefaultAsync(x => x.Id == worker.Id, stoppingToken);

            if (current is null)
                throw new EntityNotFoundException(typeof(Worker));

            var entity = WorkerMapper.DomainModelToEntity(worker);

            _dbSet.Entry(current).CurrentValues.SetValues(entity);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }
}