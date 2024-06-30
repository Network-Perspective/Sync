using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;
using NetworkPerspective.Sync.Infrastructure.Persistence.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Repositories
{
    internal class ConnectorRepository<TProperties> : IConnectorRepository<TProperties> where TProperties : ConnectorProperties, new()
    {
        private readonly DbSet<ConnectorEntity> _dbSet;

        public ConnectorRepository(DbSet<ConnectorEntity> dbSet)
        {
            _dbSet = dbSet;
        }

        public async Task AddAsync(Connector<TProperties> network, CancellationToken stoppingToken = default)
        {
            try
            {
                var entity = NetworkMapper<TProperties>.DomainModelToEntity(network);
                await _dbSet.AddAsync(entity, stoppingToken);
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
                var entity = await _dbSet.SingleAsync(x => x.Id == id, stoppingToken);
                _dbSet.Remove(entity);
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }

        public async Task<Connector<TProperties>> FindAsync(Guid id, CancellationToken stoppingToken = default)
        {
            try
            {
                var result = await _dbSet
                    .Include(x => x.Properties)
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync(stoppingToken);

                return result is null ? null : NetworkMapper<TProperties>.EntityToDomainModel(result);
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> FindPropertiesAsync(Guid id, CancellationToken stoppingToken = default)
        {
            try
            {
                var result = await _dbSet
                    .Include(x => x.Properties)
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync(stoppingToken);

                return result is null ? null : result.Properties.Select(x => new KeyValuePair<string, string>(x.Key, x.Value));
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }

        public async Task<IEnumerable<Connector<TProperties>>> GetAllAsync(CancellationToken stoppingToken = default)
        {
            try
            {
                var entities = await _dbSet
                    .Include(x => x.Properties)
                    .ToListAsync(stoppingToken);

                return entities.Select(NetworkMapper<TProperties>.EntityToDomainModel);
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }
    }
}