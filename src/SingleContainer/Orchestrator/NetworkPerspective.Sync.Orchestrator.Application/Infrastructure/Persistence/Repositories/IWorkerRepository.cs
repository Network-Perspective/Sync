using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

public interface IWorkerRepository
{
    Task AddAsync(Worker worker, CancellationToken stoppingToken = default);
}