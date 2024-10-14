using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.SecretRotation;

[DisallowConcurrentExecution]
internal class SecretRotationJob(IConnectorsService connectorsService, IWorkerRouter router, ILogger<SecretRotationJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Executing secret rotation job...");

        var connectors = await connectorsService.GetAllAsync();

        foreach (var connector in connectors)
            await TryRotateAsync(connector);
    }

    private async Task TryRotateAsync(Connector connector)
    {
        try
        {
            await router.RotateSecretsAsync(connector.Worker.Name, connector.Id, connector.Properties, connector.Type);
        }
        catch (ConnectionNotFoundException)
        {
            logger.LogWarning("Unable to rotate secrets of connector '{Id}' because hosting worker '{Name}' is not connected", connector.Id, connector.Worker.Name);
        }
    }
}