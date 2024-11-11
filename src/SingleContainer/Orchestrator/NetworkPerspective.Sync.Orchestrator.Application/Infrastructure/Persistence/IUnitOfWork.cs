using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        Task MigrateAsync();
        ISyncHistoryRepository GetSyncHistoryRepository();
        IConnectorRepository GetConnectorRepository();
        IConnectorPropertyRepository GetConnectorPropertyRepository();
        IStatusLogRepository GetStatusLogRepository();
        IWorkerRepository GetWorkerRepository();
        Task CommitAsync(CancellationToken stoppingToken = default);
        IDbSecretRepository GetDbSecretRepository();
    }
}