using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;

public class ConnectorStatus
{
    public static readonly ConnectorStatus Unknown = new(false, false, ConnectorTaskStatus.Empty);

    public bool IsAuthorized { get; set; }
    public bool IsRunning { get; set; }
    public IEnumerable<KeyValuePair<string, string>> CustomProps { get; set; }
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