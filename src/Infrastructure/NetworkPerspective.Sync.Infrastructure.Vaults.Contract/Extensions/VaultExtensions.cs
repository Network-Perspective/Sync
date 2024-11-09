using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Extensions;

public static class VaultExtensions
{
    public async static Task<bool> CanGetSecretsAsync(this IVault vault, CancellationToken stoppingToken = default, params string[] keys)
    {
        try
        {
            var tasks = keys.Select(x => vault.GetSecretAsync(x, stoppingToken));
            await Task.WhenAll(tasks);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}