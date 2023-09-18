using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Scheduler;

public class SecretRotationJob  : IJob
{
    private readonly ISecretRotator _secretRotator;

    public SecretRotationJob(ISecretRotator secretRotator)
    {
        _secretRotator = secretRotator;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        _secretRotator.RotateSecrets();
    }
}