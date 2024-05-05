using System;
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
}