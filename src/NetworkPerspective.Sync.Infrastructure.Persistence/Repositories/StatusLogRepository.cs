using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Domain.StatusLogs;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;
using NetworkPerspective.Sync.Infrastructure.Persistence.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Repositories
{
    internal class StatusLogRepository : IStatusLogRepository
    {
        private readonly DbSet<StatusLogEntity> _dbSet;

        public StatusLogRepository(DbSet<StatusLogEntity> dbSet)
        {
            _dbSet = dbSet;
        }

        public async Task AddAsync(StatusLog log, CancellationToken stoppingToken = default)
        {
            try
            {
                var entity = StatusLogMapper.DomainModelToEntity(log);
                await _dbSet.AddAsync(entity, stoppingToken);
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }

        public async Task<IEnumerable<StatusLog>> GetListAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            const int count = 50;

            try
            {

                var entities = await _dbSet
                    .Where(x => x.NetworkId == networkId)
                    .OrderByDescending(x => x.TimeStamp)
                    .Take(count)
                    .ToListAsync(stoppingToken);

                return entities.Select(StatusLogMapper.EntityToDomainModel);
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }
    }
}