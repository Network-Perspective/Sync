using System;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions
{
    public class VaultException : Exception
    {
        public VaultException(string message)
            : base(message)
        { }

        public VaultException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}