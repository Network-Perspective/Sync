using System;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;

public static class StartSyncMapper
{
    public static StartSyncDto ToDto(SyncContext syncContext)
    {
        return new StartSyncDto
        {
            CorrelationId = Guid.NewGuid(),
            Start = syncContext.Start,
            End = syncContext.End,
        };
    }
}
