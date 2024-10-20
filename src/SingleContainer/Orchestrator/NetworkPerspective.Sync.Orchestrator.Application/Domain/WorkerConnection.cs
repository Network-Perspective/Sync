namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class WorkerConnection
{
    public string Id { get; }
    public string WorkerName { get; }

    public WorkerConnection(string workerName, string id)
    {
        WorkerName = workerName;
        Id = id;
    }
}