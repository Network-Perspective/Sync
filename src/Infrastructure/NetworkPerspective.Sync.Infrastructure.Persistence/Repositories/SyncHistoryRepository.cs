using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;
using NetworkPerspective.Sync.Infrastructure.Persistence.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Repositories
{
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

        public async Task<SyncHistoryEntry> FindLastLogAsync(Guid connectorId, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _dbSet
                     .Where(x => x.Connector.Id == connectorId)
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

        public async Task RemoveAllAsync(Guid connectorId, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _dbSet
                     .Where(x => x.Connector.Id == connectorId)
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
}