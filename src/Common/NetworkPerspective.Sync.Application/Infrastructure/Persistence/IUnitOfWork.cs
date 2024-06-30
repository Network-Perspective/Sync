using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        Task MigrateAsync();
        ISyncHistoryRepository GetSyncHistoryRepository();
        IConnectorRepository<TProperties> GetConnectorRepository<TProperties>() where TProperties : ConnectorProperties, new();
        IStatusLogRepository GetStatusLogRepository();
        Task CommitAsync(CancellationToken stoppingToken = default);
        IDbSecretRepository GetDbSecretRepository();
    }
}