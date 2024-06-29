using System;

using NetworkPerspective.Sync.Application.Domain.SecretRotation;

namespace NetworkPerspective.Sync.Application.Services;

public interface ISecretRotationContextAccessor
{
    public bool IsAvailable { get; }
    public SecretRotationContext SecretRotationContext { get; set; }
}

internal class SecretRotationContextAccessor : ISecretRotationContextAccessor
{
    private readonly object _syncRoot = new();
    private SecretRotationContext _secretRotationContext = null;

    public bool IsAvailable
    {
        get
        {
            lock (_syncRoot)
                return _secretRotationContext is not null;
        }
    }

    public SecretRotationContext SecretRotationContext
    {
        get
        {
            lock (_syncRoot)
            {
                if (_secretRotationContext is null)
                    throw new NullReferenceException("Sync context is not set");

                return _secretRotationContext;
            }
        }
        set
        {
            lock (_syncRoot)
            {
                if (_secretRotationContext is not null)
                    throw new ArgumentException("Sync context is already set");

                _secretRotationContext = value;
            }
        }
    }

}