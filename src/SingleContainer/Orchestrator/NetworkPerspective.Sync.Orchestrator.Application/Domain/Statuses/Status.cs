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

public class WorkerStatus
{
    public bool IsConnected { get; private set; }
    public bool IsScheduled { get; private set; }

    private WorkerStatus(bool isConnected, bool isScheduled)
    {
        IsConnected = isConnected;
        IsScheduled = isScheduled;
    }

    public static WorkerStatus Connected(bool isScheduled)
        => new(true, isScheduled);

    public static WorkerStatus Disconnected(bool isScheduled)
        => new(false, isScheduled);
}

public class ConnectorStatus
{
    public static readonly ConnectorStatus Unknown = new(false, false, ConnectorTaskStatus.Empty);

    public bool IsAuthorized { get; set; }
    public bool IsRunning { get; set; }
    public ConnectorTaskStatus CurrentTask { get; set; }

    private ConnectorStatus(bool isAuthorized, bool isRunning, ConnectorTaskStatus currentTask)
    {
        IsAuthorized = isAuthorized;
        IsRunning = isRunning;
        CurrentTask = currentTask;
    }

    public static ConnectorStatus Running(bool isAuthorized, ConnectorTaskStatus currentTask)
        => new(isAuthorized, true, currentTask);

    public static ConnectorStatus Idle(bool isAuthorized)
    => new(isAuthorized, false, ConnectorTaskStatus.Empty);
}

public class ConnectorTaskStatus
{
    public static readonly ConnectorTaskStatus Empty = new(string.Empty, string.Empty, null);

    public string Caption { get; set; }
    public string Description { get; set; }
    public double? CompletionRate { get; set; }

    public ConnectorTaskStatus(string caption, string description, double? completionRate)
    {
        Caption = caption;
        Description = description;
        CompletionRate = completionRate;
    }

    public override string ToString()
    {
        var completionRate = CompletionRate is null ? "???" : $"{CompletionRate: 0.##}%";
        return $"{Caption}: {completionRate} ({Description})";
    }
}