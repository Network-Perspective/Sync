using System;

namespace NetworkPerspective.Sync.Application.Domain.Sync
{
    public class SyncHistoryEntry
    {
        public Guid NetworkId { get; }
        public DateTime TimeStamp { get; }
        public TimeRange SyncPeriod { get; }

        public SyncHistoryEntry(Guid networkId, DateTime timeStamp, TimeRange syncPeriod)
        {
            NetworkId = networkId;
            TimeStamp = timeStamp;
            SyncPeriod = syncPeriod;
        }
    }
}