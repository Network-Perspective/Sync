using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Mappers
{
    internal static class SyncHistoryEntryMapper
    {
        public static SyncHistoryEntry EntityToDomainModel(SyncHistoryEntryEntity entity)
        {
            return SyncHistoryEntry.Create(entity.ConnectorId, entity.TimeStamp, new TimeRange(entity.SyncPeriodStart, entity.SyncPeriodEnd));
        }

        public static SyncHistoryEntryEntity DomainModelToEntity(SyncHistoryEntry syncHistoryEntry)
        {
            return new SyncHistoryEntryEntity
            {
                ConnectorId = syncHistoryEntry.ConnectorId,
                TimeStamp = syncHistoryEntry.TimeStamp,
                SyncPeriodStart = syncHistoryEntry.SyncPeriod.Start,
                SyncPeriodEnd = syncHistoryEntry.SyncPeriod.End,
                SuccessRate = syncHistoryEntry.Result.SuccessRate,
                TasksCount = syncHistoryEntry.Result.TasksCount,
                InteractionsCount = syncHistoryEntry.Result.TotalInteractionsCount,
            };
        }
    }
}