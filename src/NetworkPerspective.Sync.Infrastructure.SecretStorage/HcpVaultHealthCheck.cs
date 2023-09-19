using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage;

public class HcpVaultHealthCheck : IHealthCheck
{
    private readonly HcpVaultClient _hcpVaultClient;
    private readonly string _testSecretName;
    private readonly string _url;


    public HcpVaultHealthCheck(HcpVaultClient hcpVaultClient, IOptions<HcpVaultConfig> config)
    {
        _hcpVaultClient = hcpVaultClient;
        _testSecretName = config.Value.TestSecretName;
        _url = config.Value.BaseUrl;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            await _hcpVaultClient.GetSecretAsync(_testSecretName, cancellationToken);
            return HealthCheckResult.Healthy("HCPVault healthy");
        }
        catch
        {
            return HealthCheckResult.Unhealthy($"HCPVault unhealthy at {_url}");
        }
    }
}