using System;

namespace NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

public interface ISecretRepositoryFactory
{
    ISecretRepository Create(Uri externalKeyVaultUri = null);
}