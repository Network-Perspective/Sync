using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Services;

public interface ISecretRotationScheduler
{
    Task ScheduleSecretsRotation();
}