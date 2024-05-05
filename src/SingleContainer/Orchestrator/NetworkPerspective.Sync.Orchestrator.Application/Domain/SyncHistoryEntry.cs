using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class SyncHistoryEntry
{
    public Guid ConnectorId { get; }
    public DateTime TimeStamp { get; }
    public TimeRange SyncPeriod { get; }
    public SyncResult Result { get; }

    private SyncHistoryEntry(Guid connectorId, DateTime timeStamp, TimeRange syncPeriod, SyncResult result)
    {
        ConnectorId = connectorId;
        TimeStamp = timeStamp;
        SyncPeriod = syncPeriod;
        Result = result;
    }

    public static SyncHistoryEntry Create(Guid connectorId, DateTime timeStamp, TimeRange syncPeriod)
        => new SyncHistoryEntry(connectorId, timeStamp, syncPeriod, SyncResult.Empty);

    public static SyncHistoryEntry CreateWithResult(Guid connectorId, DateTime timeStamp, TimeRange syncPeriod, SyncResult syncResult)
        => new SyncHistoryEntry(connectorId, timeStamp, syncPeriod, syncResult);
}