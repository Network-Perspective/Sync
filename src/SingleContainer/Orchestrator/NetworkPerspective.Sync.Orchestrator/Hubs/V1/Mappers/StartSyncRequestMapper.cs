using System;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;

public static class StartSyncMapper
{
    public static StartSyncDto ToDto(SyncContext syncContext)
    {
        return new StartSyncDto
        {
            CorrelationId = Guid.NewGuid(),
            ConnectorId = syncContext.ConnectorId,
            ConnectorType = syncContext.ConnectorType,
            NetworkId = syncContext.NetworkId,
            Start = syncContext.TimeRange.Start,
            End = syncContext.TimeRange.End,
            AccessToken = syncContext.AccessToken.ToSystemString(),
            NetworkProperties = syncContext.NetworkProperties,
        };
    }
}