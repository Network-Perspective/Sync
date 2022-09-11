using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Mappers
{
    internal static class SyncHistoryEntryMapper
    {
        public static SyncHistoryEntry EntityToDomainModel(SyncHistoryEntryEntity entity)
            => new SyncHistoryEntry(entity.NetworkId, entity.TimeStamp, new TimeRange(entity.SyncPeriodStart, entity.SyncPeriodEnd));

        public static SyncHistoryEntryEntity DomainModelToEntity(SyncHistoryEntry syncHistoryEntry)
        {
            return new SyncHistoryEntryEntity
            {
                NetworkId = syncHistoryEntry.NetworkId,
                TimeStamp = syncHistoryEntry.TimeStamp,
                SyncPeriodStart = syncHistoryEntry.SyncPeriod.Start,
                SyncPeriodEnd = syncHistoryEntry.SyncPeriod.End,
            };
        }
    }
}