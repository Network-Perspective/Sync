using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;
using NetworkPerspective.Sync.Infrastructure.Persistence.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Repositories
{
    internal class NetworkRepository<TProperties> : INetworkRepository<TProperties> where TProperties : NetworkProperties, new()
    {
        private readonly DbSet<NetworkEntity> _dbSet;

        public NetworkRepository(DbSet<NetworkEntity> dbSet)
        {
            _dbSet = dbSet;
        }

        public async Task AddAsync(Network<TProperties> network, CancellationToken stoppingToken = default)
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

        public async Task<Network<TProperties>> FindAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                var result = await _dbSet
                    .Include(x => x.Properties)
                    .Where(x => x.Id == networkId)
                    .FirstOrDefaultAsync(stoppingToken);

                return result is null ? null : NetworkMapper<TProperties>.EntityToDomainModel(result);
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }

        public async Task<IEnumerable<Network<TProperties>>> GetAllAsync(CancellationToken stoppingToken = default)
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