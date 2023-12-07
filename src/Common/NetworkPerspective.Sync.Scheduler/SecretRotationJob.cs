using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Scheduler;

[DisallowConcurrentExecution]
internal class SecretRotationJob : IJob
{
    private readonly ISecretRotator _secretRotator;

    public SecretRotationJob(ISecretRotator secretRotator)
    {
        _secretRotator = secretRotator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _secretRotator.RotateSecrets();
    }
}