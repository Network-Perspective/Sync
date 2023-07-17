using System;

namespace NetworkPerspective.Sync.Application.Domain.Sync
{
    public class SyncHistoryEntry
    {
        public Guid NetworkId { get; }
        public DateTime TimeStamp { get; }
        public TimeRange SyncPeriod { get; }
        public SyncResult Result { get; }

        private SyncHistoryEntry(Guid networkId, DateTime timeStamp, TimeRange syncPeriod, SyncResult result)
        {
            NetworkId = networkId;
            TimeStamp = timeStamp;
            SyncPeriod = syncPeriod;
            Result = result;
        }

        public static SyncHistoryEntry Create(Guid networkId, DateTime timeStamp, TimeRange syncPeriod)
            => new SyncHistoryEntry(networkId, timeStamp, syncPeriod, SyncResult.Empty);

        public static SyncHistoryEntry CreateWithResult(Guid networkId, DateTime timeStamp, TimeRange syncPeriod, SyncResult syncResult)
            => new SyncHistoryEntry(networkId, timeStamp, syncPeriod, syncResult);
    }
}