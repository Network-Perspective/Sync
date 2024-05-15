using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

public interface IWorkerRepository
{
    Task<IEnumerable<Worker>> GetAllAsync(CancellationToken stoppingToken = default);
    Task<Worker> GetAsync(Guid id, CancellationToken stoppingToken = default);
    Task<Worker> GetAsync(string name, CancellationToken stoppingToken = default);
    Task AddAsync(Worker worker, CancellationToken stoppingToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken stoppingToken = default);
    Task RemoveAsync(Guid id, CancellationToken stoppingToken = default);
    Task UpdateAsync(Worker worker, CancellationToken stoppingToken = default);
}