using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Mappers;

internal static class SyncHistoryEntryMapper
{
    public static SyncHistoryEntry EntityToDomainModel(SyncHistoryEntryEntity entity)
    {
        return SyncHistoryEntry.Create(entity.DataSourceId, entity.TimeStamp, new TimeRange(entity.SyncPeriodStart, entity.SyncPeriodEnd));
    }

    public static SyncHistoryEntryEntity DomainModelToEntity(SyncHistoryEntry syncHistoryEntry)
    {
        return new SyncHistoryEntryEntity
        {
            DataSourceId = syncHistoryEntry.NetworkId,
            TimeStamp = syncHistoryEntry.TimeStamp,
            SyncPeriodStart = syncHistoryEntry.SyncPeriod.Start,
            SyncPeriodEnd = syncHistoryEntry.SyncPeriod.End,
            SuccessRate = syncHistoryEntry.Result.SuccessRate,
            TasksCount = syncHistoryEntry.Result.TasksCount,
            InteractionsCount = syncHistoryEntry.Result.TotalInteractionsCount,
        };
    }
}