using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

public interface IDbSecretRepository
{
    Task<string> GetSecretAsync(string key, CancellationToken stoppingToken = default);
    Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default);
    Task SetSecretAsync(string key, string encrypted, CancellationToken stoppingToken = default);
}