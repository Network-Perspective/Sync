using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage;

public class DbSecretRepositoryHealthCheck : IHealthCheck
{
    private readonly DbSecretRepositoryClient _client;

    public DbSecretRepositoryHealthCheck(DbSecretRepositoryClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            _ = _client.Encrypt("test".ToSecureString());
        }
        catch
        {
            return HealthCheckResult.Unhealthy("DbSecretRepository missing keys");
        }

        try
        {
            await _client.SetSecretAsync("test", "secret".ToSecureString(), cancellationToken);
            var value = await _client.GetSecretAsync("test", cancellationToken);
            if (value.ToSystemString() != "secret")
                return HealthCheckResult.Unhealthy("DbSecretRepository unhealthy - secret value mismatch");

            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy("DbSecretRepository store unhealthy");
        }
    }
}