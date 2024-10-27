using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Worker.HostedServices;

internal class StartupHealthChecker(HealthCheckService healthCheckService, ILogger<StartupHealthChecker> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var result = await healthCheckService.CheckHealthAsync(cancellationToken);

        var unhealthyDeps = result.Entries.Where(x => x.Value.Status == HealthStatus.Unhealthy);
        var degradedDeps = result.Entries.Where(x => x.Value.Status == HealthStatus.Degraded);
        var healthyDeps = result.Entries.Where(x => x.Value.Status == HealthStatus.Healthy);

        foreach (var dep in unhealthyDeps)
            logger.LogError("{key}: unhealthy ({reason})", dep.Key, dep.Value.Description);

        foreach (var dep in degradedDeps)
            logger.LogWarning("{key}: degraded ({reason})", dep.Key, dep.Value.Description);

        foreach (var dep in healthyDeps)
            logger.LogInformation("{key}: healthy", dep.Key);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}