using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace NetworkPerspective.Sync.Infrastructure.Core.HealthChecks
{
    internal class NetworkPerspectiveCoreHealthCheck : IHealthCheck
    {
        private readonly NetworkPerspectiveCoreConfig _config;

        public NetworkPerspectiveCoreHealthCheck(IOptions<NetworkPerspectiveCoreConfig> config)
        {
            _config = config.Value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy($"At {_config.BaseUrl}. Not implemented yet..."));
        }
    }
}