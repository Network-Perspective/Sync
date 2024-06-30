using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract;

public interface IVault
{
    Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default);
    Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default);
    Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default);
}