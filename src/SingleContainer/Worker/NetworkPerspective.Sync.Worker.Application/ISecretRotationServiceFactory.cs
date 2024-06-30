using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application;

public interface ISecretRotationServiceFactory
{
    ISecretRotationService CreateSecretRotator();
}

internal class SecretRotatorFactory : ISecretRotationServiceFactory
{
    private readonly IEnumerable<ISecretRotationService> _secretRotators;

    public SecretRotatorFactory(IEnumerable<ISecretRotationService> secretRotators)
    {
        _secretRotators = secretRotators;
    }

    public ISecretRotationService CreateSecretRotator()
    {
        return _secretRotators.Single();
    }
}