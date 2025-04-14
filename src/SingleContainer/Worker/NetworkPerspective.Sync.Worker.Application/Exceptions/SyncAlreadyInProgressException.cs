using System;

namespace NetworkPerspective.Sync.Worker.Application.Exceptions;

internal class SyncAlreadyInProgressException(Guid connectorId)
    : ApplicationException($"Connector '{connectorId}' synchronization already in progress")
{
}