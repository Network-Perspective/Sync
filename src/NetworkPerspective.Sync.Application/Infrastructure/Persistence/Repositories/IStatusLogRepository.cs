using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.StatusLogs;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories
{
    public interface IStatusLogRepository
    {
        Task<IEnumerable<StatusLog>> GetListAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task AddAsync(StatusLog log, CancellationToken stoppingToken = default);
    }
}