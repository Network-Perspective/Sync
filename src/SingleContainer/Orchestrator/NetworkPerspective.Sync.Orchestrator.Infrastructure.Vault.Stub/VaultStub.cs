using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Stub;

internal class VaultStub : IVault
{
    public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        await Task.Yield();

        if (string.Equals(key, "orchestrator-api-key"))
            return "api-key".ToSecureString();

        throw new ArgumentOutOfRangeException();
    }
}