using System;

using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class SyncHistoryEntry
{
    public Guid ConnectorId { get; }
    public DateTime TimeStamp { get; }
    public TimeRange SyncPeriod { get; }
    public double SuccessRate { get; }
    public int TasksCount { get; }
    public long TotalInteractionsCount { get; }

    private SyncHistoryEntry(Guid connectorId, DateTime timeStamp, TimeRange syncPeriod, double successRate, int tasksCount, long totalInteractionsCount)
    {
        ConnectorId = connectorId;
        TimeStamp = timeStamp;
        SyncPeriod = syncPeriod;
        SuccessRate = successRate;
        TasksCount = tasksCount;
        TotalInteractionsCount = totalInteractionsCount;
    }

    public static SyncHistoryEntry Create(Guid connectorId, DateTime timeStamp, TimeRange syncPeriod, double successRate, int tasksCount, long totalInteractionsCount)
        => new SyncHistoryEntry(connectorId, timeStamp, syncPeriod, successRate, tasksCount, totalInteractionsCount);

    public static SyncHistoryEntry CreateWithEmptyResult(Guid connectorId, DateTime timeStamp, TimeRange syncPeriod)
        => CreateWithResult(connectorId, timeStamp, syncPeriod, SyncResult.Empty);

    public static SyncHistoryEntry CreateWithResult(Guid connectorId, DateTime timeStamp, TimeRange syncPeriod, SyncResult syncResult)
        => new SyncHistoryEntry(connectorId, timeStamp, syncPeriod, syncResult.SuccessRate, syncResult.TasksCount, syncResult.TotalInteractionsCount);
}