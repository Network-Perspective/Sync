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

internal class WorkerRepository : IWorkerRepository
{
    private readonly DbSet<WorkerEntity> _dbSet;

    public WorkerRepository(DbSet<WorkerEntity> dbSet)
    {
        _dbSet = dbSet;
    }

    public async Task AddAsync(Worker worker, CancellationToken stoppingToken = default)
    {
        if (await ExistsAsync(worker.Name, stoppingToken))
            throw new EntityAlreadyExistsException($"'{typeof(Worker)}' with a name '{worker.Name}' already exists");

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
                .SingleOrDefaultAsync(x => x.Id == id, stoppingToken);

            return result is not null;
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await _dbSet
                .SingleOrDefaultAsync(x => x.Name == name, stoppingToken);

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
        if (!await ExistsAsync(id, stoppingToken))
            throw new EntityNotFoundException<Worker>();

        try
        {
            var result = await _dbSet
                .SingleAsync(x => x.Id == id, stoppingToken);

            return WorkerMapper.EntityToDomainModel(result);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<Worker> GetAsync(string name, CancellationToken stoppingToken = default)
    {
        if (!await ExistsAsync(name, stoppingToken))
            throw new EntityNotFoundException<Worker>();

        try
        {
            var result = await _dbSet
                .SingleAsync(x => x.Name == name, stoppingToken);

            return WorkerMapper.EntityToDomainModel(result);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken stoppingToken = default)
    {
        if (!await ExistsAsync(id, stoppingToken))
            throw new EntityNotFoundException<Worker>();

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
                throw new EntityNotFoundException<Worker>();

            var entity = WorkerMapper.DomainModelToEntity(worker);

            _dbSet.Entry(current).CurrentValues.SetValues(entity);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }
}