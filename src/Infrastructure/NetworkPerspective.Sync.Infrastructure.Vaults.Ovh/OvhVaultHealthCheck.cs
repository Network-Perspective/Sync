using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.Ovh;

internal class OvhVaultHealthCheck(OvhVaultClient client, IOptions<OvhVaultConfig> config) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var secret = await client.GetSecretAsync(config.Value.TestSecretName, cancellationToken);
            return HealthCheckResult.Healthy("OVH vault healthy");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("OVH vault secret read failed");
        }
    }
}