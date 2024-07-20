﻿using NetworkPerspective.Sync.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Application.Exceptions;

public class SyncContextNotAvailableException : ApplicationException
{
    public SyncContextNotAvailableException() : base($"{nameof(SyncContext)} is not avaibale. Make sure You attempt to invoke it in context of synchronization")
    { }
}
