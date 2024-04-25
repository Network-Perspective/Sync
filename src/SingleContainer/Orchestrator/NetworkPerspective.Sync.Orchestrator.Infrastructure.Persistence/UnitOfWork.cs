using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Repositories;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;

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

    public IDataSourceRepository<TProperties> GetDataSourceRepository<TProperties>() where TProperties : DataSourceProperties, new()
        => new DataSourceRepository<TProperties>(_dbContext.NetworkEntities);

    public IStatusLogRepository GetStatusLogRepository()
        => new StatusLogRepository(_dbContext.StatusLogEntities);

    public IDbSecretRepository GetDbSecretRepository()
        => new DbSecretRepository(_dbContext.SecretEntities);

    public Task MigrateAsync()
        => _dbContext.Database.MigrateAsync();

    public void Dispose()
        => _dbContext?.Dispose();
}