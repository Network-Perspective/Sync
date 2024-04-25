using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories
{
    public interface ISyncHistoryRepository
    {
        Task<SyncHistoryEntry> FindLastLogAsync(Guid networkId, CancellationToken cancellationToken = default);
        Task AddAsync(SyncHistoryEntry log, CancellationToken cancellationToken = default);
        Task RemoveAllAsync(Guid networkId, CancellationToken cancellationToken = default);
    }
}