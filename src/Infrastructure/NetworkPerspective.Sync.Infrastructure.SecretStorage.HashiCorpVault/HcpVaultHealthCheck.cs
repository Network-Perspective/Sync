using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.HashiCorpVault;

internal class HcpVaultHealthCheck : IHealthCheck
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

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
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