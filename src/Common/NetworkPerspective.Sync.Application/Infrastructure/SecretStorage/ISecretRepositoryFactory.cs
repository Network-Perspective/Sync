using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Infrastructure.SecretStorage
{
    public interface ISecretRepositoryFactory
    {
        Task<ISecretRepository> CreateAsync(Guid connectorId, CancellationToken stoppingToken = default);
        public ISecretRepository CreateDefault();
    }
}