using NetworkPerspective.Sync.Worker.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Worker.Application.Exceptions;

public class SyncContextAlreadyInitializedException : ApplicationException
{
    public SyncContextAlreadyInitializedException() : base($"{nameof(SyncContext)} already initialized")
    { }
}