using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Infrastructure.SecretStorage
{
    public interface ISecretRepository
    {
        public Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default);
        public Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default);
        public Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default);
    }
}