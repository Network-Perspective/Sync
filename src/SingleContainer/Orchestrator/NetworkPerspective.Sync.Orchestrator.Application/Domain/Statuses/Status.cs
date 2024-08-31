using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;

public class Status
{
    public WorkerStatus WorkerStatus { get; private set; }
    public ConnectorStatus ConnectorStatus { get; private set; }
    public IEnumerable<StatusLog> Logs { get; private set; }

    private Status(WorkerStatus workerStatus, ConnectorStatus connectorStatus, IEnumerable<StatusLog> logs)
    {
        WorkerStatus = workerStatus;
        ConnectorStatus = connectorStatus;
        Logs = logs;
    }

    public static Status Disconnected(bool isScheduled, IEnumerable<StatusLog> logs)
        => new(WorkerStatus.Disconnected(isScheduled), ConnectorStatus.Unknown, logs);

    public static Status Connected(bool isScheduled, ConnectorStatus connectorStatus, IEnumerable<StatusLog> logs)
        => new(WorkerStatus.Connected(isScheduled), connectorStatus, logs);
}