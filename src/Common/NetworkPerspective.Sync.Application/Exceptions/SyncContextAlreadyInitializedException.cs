using NetworkPerspective.Sync.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Application.Exceptions;

public class SyncContextAlreadyInitializedException : ApplicationException
{
    public SyncContextAlreadyInitializedException() : base($"{nameof(SyncContext)} already initialized")
    { }
}