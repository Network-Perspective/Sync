using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{

    internal class DbSecretRepository : IDbSecretRepository
    {
        private readonly DbSet<SecretEntity> _secretEntities;

        public DbSecretRepository(DbSet<SecretEntity> secretEntities)
        {
            _secretEntities = secretEntities;
        }

        public async Task<string> GetSecretAsync(string key, CancellationToken stoppingToken = default)
        {
            try
            {
                var secret = await _secretEntities.FirstOrDefaultAsync(s => s.Key == key);
                return secret?.Value;
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }

        public async Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
        {
            try
            {
                var secret = await _secretEntities.FirstOrDefaultAsync(s => s.Key == key);
                _secretEntities.Remove(secret);
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }

        public async Task SetSecretAsync(string key, string encrypted, CancellationToken stoppingToken = default)
        {
            try
            {
                var secretEntity = await _secretEntities.FirstOrDefaultAsync(s => s.Key == key);
                if (secretEntity == null)
                {
                    secretEntity = new SecretEntity()
                    {
                        Id = Guid.NewGuid(),
                        Key = key,
                        Value = encrypted,
                    };
                    _secretEntities.Add(secretEntity);
                }
                else
                {
                    secretEntity.Value = encrypted;
                }
            }
            catch (Exception ex)
            {
                throw new DbException(ex);
            }
        }

    }
}
