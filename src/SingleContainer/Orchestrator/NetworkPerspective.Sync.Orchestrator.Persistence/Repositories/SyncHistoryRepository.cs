using System;
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

internal class SyncHistoryRepository : ISyncHistoryRepository
{
    private readonly DbSet<SyncHistoryEntryEntity> _dbSet;

    public SyncHistoryRepository(DbSet<SyncHistoryEntryEntity> dbSet)
    {
        _dbSet = dbSet;
    }

    public async Task AddAsync(SyncHistoryEntry log, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = SyncHistoryEntryMapper.DomainModelToEntity(log);
            await _dbSet.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task<SyncHistoryEntry> FindLastLogAsync(Guid networkId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet
                 .Where(x => x.Connector.Id == networkId)
                 .OrderByDescending(x => x.TimeStamp)
                 .FirstOrDefaultAsync();

            if (entity == null)
                return null;

            return SyncHistoryEntryMapper.EntityToDomainModel(entity);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }

    public async Task RemoveAllAsync(Guid networkId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet
                 .Where(x => x.Connector.Id == networkId)
                 .ToListAsync(cancellationToken);

            _dbSet
                .RemoveRange(entity);
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }
}