using NetworkPerspective.Sync.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncContextProvider
    {
        SyncContext Context { get; }
    }
}
