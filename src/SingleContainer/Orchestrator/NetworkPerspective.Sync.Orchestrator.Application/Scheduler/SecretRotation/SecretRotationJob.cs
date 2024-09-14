using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.SecretRotation;

[DisallowConcurrentExecution]
internal class SecretRotationJob : IJob
{
    private readonly IConnectorsService _connectorsService;
    private readonly IWorkerRouter _router;
    private readonly ILogger<SecretRotationJob> _logger;

    public SecretRotationJob(IConnectorsService connectorsService, IWorkerRouter router, ILogger<SecretRotationJob> logger)
    {
        _connectorsService = connectorsService;
        _router = router;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Executing secret rotation job...");

        var connectors = await _connectorsService.GetAllAsync();

        foreach (var connector in connectors)
            await _router.RotateSecretsAsync(connector.Worker.Name, connector.Id, connector.Properties, connector.Type);
    }
}