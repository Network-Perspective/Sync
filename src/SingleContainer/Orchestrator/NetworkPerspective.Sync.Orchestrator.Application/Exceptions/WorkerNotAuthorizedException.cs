using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Exceptions;

public class WorkerNotAuthorizedException : Exception
{
    public WorkerNotAuthorizedException(string name)
        : base($"Worker '{name}' is not authorized")
    { }
}
