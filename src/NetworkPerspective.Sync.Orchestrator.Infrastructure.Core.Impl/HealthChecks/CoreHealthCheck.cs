using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Impl;

namespace NetworkPerspective.Sync.Infrastructure.Core.HealthChecks
{
    internal class CoreHealthCheck : IHealthCheck
    {
        private readonly CoreConfig _config;
        private readonly ISettingsClient _client;

        public CoreHealthCheck(ISettingsClient client, IOptions<CoreConfig> config)
        {
            _config = config.Value;
            _client = client;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var coreHealth = await _client.HealthAsync(cancellationToken);
                if (coreHealth.Healthy == true)
                    return HealthCheckResult.Healthy($"Healthy at {_config.BaseUrl}");
                else
                    return HealthCheckResult.Unhealthy($"Core connected but unhealthy at {_config.BaseUrl}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Error establishing connection to core at {_config.BaseUrl}: {ex.Message}");
            }
        }
    }
}