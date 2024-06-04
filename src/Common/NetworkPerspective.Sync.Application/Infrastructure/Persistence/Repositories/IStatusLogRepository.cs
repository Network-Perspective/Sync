using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories
{
    public interface IStatusLogRepository
    {
        Task<IEnumerable<StatusLog>> GetListAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task AddAsync(StatusLog log, CancellationToken stoppingToken = default);
    }
}