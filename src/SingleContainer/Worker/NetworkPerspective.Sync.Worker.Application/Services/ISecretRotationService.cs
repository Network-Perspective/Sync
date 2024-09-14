using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.SecretRotation;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ISecretRotationService
{
    Task ExecuteAsync(SecretRotationContext context, CancellationToken stoppingToken = default);
}