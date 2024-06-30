using System;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.DbVault;

internal class DbSecretRepositoryClientFactory : ISecretRepositoryFactory
{
    private readonly DbSecretRepositoryClient _secretReposiroty;

    public DbSecretRepositoryClientFactory(DbSecretRepositoryClient secretReposiroty)
    {
        _secretReposiroty = secretReposiroty;
    }

    public ISecretRepository Create(Uri externalKeyVaultUri = null)
    {
        if (externalKeyVaultUri is not null)
            throw new Exception();

        return _secretReposiroty;
    }
}