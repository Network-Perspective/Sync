using System;

namespace NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions
{
    public class SecretStorageException : Exception
    {
        public SecretStorageException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}