﻿using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Mappers;

internal static class SyncHistoryEntryMapper
{
    public static SyncHistoryEntry EntityToDomainModel(SyncHistoryEntryEntity entity)
    {
        return SyncHistoryEntry.Create(entity.ConnectorId, entity.TimeStamp, new TimeRange(entity.SyncPeriodStart, entity.SyncPeriodEnd), entity.SuccessRate ?? 0, entity.TasksCount, entity.InteractionsCount);
    }

    public static SyncHistoryEntryEntity DomainModelToEntity(SyncHistoryEntry syncHistoryEntry)
    {
        return new SyncHistoryEntryEntity
        {
            ConnectorId = syncHistoryEntry.ConnectorId,
            TimeStamp = syncHistoryEntry.TimeStamp,
            SyncPeriodStart = syncHistoryEntry.SyncPeriod.Start,
            SyncPeriodEnd = syncHistoryEntry.SyncPeriod.End,
            SuccessRate = syncHistoryEntry.SuccessRate,
            TasksCount = syncHistoryEntry.TasksCount,
            InteractionsCount = syncHistoryEntry.TotalInteractionsCount,
        };
    }
}