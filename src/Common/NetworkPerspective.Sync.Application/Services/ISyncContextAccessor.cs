using System;
using NetworkPerspective.Sync.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Application.Services;

public interface ISyncContextAccessor
{
    public bool IsAvailable { get; }
    public SyncContext SyncContext { get; set; }
}

internal class SyncContextAccessor : ISyncContextAccessor
{
    private readonly object _syncRoot = new();
    private SyncContext _syncContext = null;

    public bool IsAvailable
    {
        get
        {
            lock (_syncRoot)
                return _syncContext is not null;
        }
    }


    public SyncContext SyncContext 
    {
        get
        {
            lock (_syncRoot)
            {
                if (_syncContext is null)
                    throw new NullReferenceException("Sync context is not set");

                return _syncContext;
            }
        }
        set
        {
            lock (_syncRoot)
            {
                if (_syncContext is not null)
                    throw new ArgumentException("Sync context is already set");

                _syncContext = value;
            }
        }
    }
}
