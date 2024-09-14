using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.Core.HealthChecks
{
    internal class NetworkPerspectiveCoreHealthCheck : IHealthCheck
    {
        private readonly NetworkPerspectiveCoreConfig _config;

        public NetworkPerspectiveCoreHealthCheck(IOptions<NetworkPerspectiveCoreConfig> config)
        {
            _config = config.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // configure client
                using var http = new System.Net.Http.HttpClient();
                http.BaseAddress = new Uri(_config.BaseUrl);
                var client = new SettingsClient(http);

                // heath check core
                var coreHealth = await client.HealthAsync();
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