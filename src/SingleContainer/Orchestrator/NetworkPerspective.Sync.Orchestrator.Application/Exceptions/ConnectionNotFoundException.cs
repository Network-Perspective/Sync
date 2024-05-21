using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Exceptions;

public class ConnectionNotFoundException : Exception
{
    public ConnectionNotFoundException(string workerName)
        : base($"There is no connection for worker '{workerName}'")
    { }
}
