using System;

namespace NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions
{
    public class NetworkPerspectiveCoreException : Exception
    {
        public NetworkPerspectiveCoreException(string url, Exception innerException)
            : base($"Unsuccessfull request to Network Perspective Core at '{url}'. Please see inner exception", innerException)
        { }

        public NetworkPerspectiveCoreException(string message)
            : base(message)
        { }
    }
}