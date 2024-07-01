using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

public interface IStatusLogRepository
{
    Task<IEnumerable<StatusLog>> GetListAsync(Guid networkId, CancellationToken stoppingToken = default);
    Task AddAsync(StatusLog log, CancellationToken stoppingToken = default);
}