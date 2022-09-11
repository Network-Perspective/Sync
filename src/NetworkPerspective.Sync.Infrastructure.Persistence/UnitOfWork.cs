using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Infrastructure.Persistence.Repositories;

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

        public INetworkRepository<TProperties> GetNetworkRepository<TProperties>() where TProperties : NetworkProperties, new()
            => new NetworkRepository<TProperties>(_dbContext.NetworkEntities);

        public IStatusLogRepository GetStatusLogRepository()
            => new StatusLogRepository(_dbContext.StatusLogEntities);

        public Task MigrateAsync()
            => _dbContext.Database.MigrateAsync();

        public void Dispose()
            => _dbContext?.Dispose();
    }
}