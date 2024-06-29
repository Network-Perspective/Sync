using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.SecretRotation;

namespace NetworkPerspective.Sync.Application.Services;

public interface ISecretRotationService
{
    Task ExecuteAsync(SecretRotationContext context, CancellationToken stoppingToken = default);
}