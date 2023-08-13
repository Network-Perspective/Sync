using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace NetworkPerspective.Sync.Infrastructure.Core.HealthChecks
{
    internal class NetworkPerspectiveCoreHealthCheck : IHealthCheck
    {
        private readonly NetworkPerspectiveCoreConfig _config;
        private readonly ISettingsClient _coreClient;

        public NetworkPerspectiveCoreHealthCheck(IOptions<NetworkPerspectiveCoreConfig> config, ISettingsClient coreClient)
        {
            _config = config.Value;
            _coreClient = coreClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var coreHealth = await _coreClient.HealthAsync();
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