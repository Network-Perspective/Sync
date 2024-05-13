using System;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract.Exceptions;

public class VaultException : Exception
{
    public VaultException(string message, Exception innerException)
        : base(message, innerException)
    { }
}