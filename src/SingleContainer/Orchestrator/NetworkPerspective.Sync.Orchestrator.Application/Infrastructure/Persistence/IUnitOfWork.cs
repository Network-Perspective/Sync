using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        Task MigrateAsync();
        ISyncHistoryRepository GetSyncHistoryRepository();
        IDataSourceRepository<TProperties> GetDataSourceRepository<TProperties>() where TProperties : DataSourceProperties, new();
        IStatusLogRepository GetStatusLogRepository();
        Task CommitAsync(CancellationToken stoppingToken = default);
        IDbSecretRepository GetDbSecretRepository();
    }
}