namespace NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;

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