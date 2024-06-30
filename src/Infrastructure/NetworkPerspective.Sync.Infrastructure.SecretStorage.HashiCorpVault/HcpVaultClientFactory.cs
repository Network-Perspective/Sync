using System;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.HashiCorpVault;

internal class HcpVaultClientFactory : ISecretRepositoryFactory
{
    private readonly HcpVaultClient _hcpVaultClient;

    public HcpVaultClientFactory(HcpVaultClient hcpVaultClient)
    {
        _hcpVaultClient = hcpVaultClient;
    }

    public ISecretRepository Create(Uri externalKeyVaultUri = null)
    {
        if (externalKeyVaultUri is not null)
            throw new Exception();

        return _hcpVaultClient;
    }
}