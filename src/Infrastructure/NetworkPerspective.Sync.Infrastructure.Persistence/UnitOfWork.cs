using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Infrastructure.Vaults;

namespace NetworkPerspective.Sync.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConnectorDbContext _dbContext;

        public UnitOfWork(DbContextOptions<ConnectorDbContext> options)
        {
            _dbContext = new ConnectorDbContext(options);
        }

        public Task CommitAsync(CancellationToken stoppingToken = default)
            => _dbContext.SaveChangesAsync(stoppingToken);

        public ISyncHistoryRepository GetSyncHistoryRepository()
            => new SyncHistoryRepository(_dbContext.SyncHistoryEntities);

        public IConnectorRepository<TProperties> GetConnectorRepository<TProperties>() where TProperties : ConnectorProperties, new()
            => new ConnectorRepository<TProperties>(_dbContext.NetworkEntities);

        public IStatusLogRepository GetStatusLogRepository()
            => new StatusLogRepository(_dbContext.StatusLogEntities);

        public IDbSecretRepository GetDbSecretRepository()
            => new DbSecretRepository(_dbContext.SecretEntities);

        public Task MigrateAsync()
            => _dbContext.Database.MigrateAsync();

        public void Dispose()
            => _dbContext?.Dispose();


    }
}