using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        Task MigrateAsync();
        ISyncHistoryRepository GetSyncHistoryRepository();
        INetworkRepository<TProperties> GetNetworkRepository<TProperties>() where TProperties : NetworkProperties, new();
        IStatusLogRepository GetStatusLogRepository();
        Task CommitAsync(CancellationToken stoppingToken = default);
        IDbSecretRepository GetDbSecretRepository();
    }
}