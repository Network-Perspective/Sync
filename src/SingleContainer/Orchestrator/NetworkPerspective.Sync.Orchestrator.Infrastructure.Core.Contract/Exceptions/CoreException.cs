using System;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract.Exceptions;

public class CoreException : Exception
{
    public CoreException(string url, Exception innerException)
    : base($"Unsuccessfull request to Network Perspective Core at '{url}'. Please see inner exception", innerException)
    { }

    public CoreException(string message)
        : base(message)
    { }
}