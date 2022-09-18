using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories
{
    public interface ISyncHistoryRepository
    {
        Task<SyncHistoryEntry> FindLastLogAsync(Guid networkId, CancellationToken cancellationToken = default);
        Task AddAsync(SyncHistoryEntry log, CancellationToken cancellationToken = default);
    }
}