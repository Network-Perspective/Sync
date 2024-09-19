using System;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira;

public class JiraAuthProcess
{
    public Guid ConnectorId { get; }
    public string WorkerName { get; }
    public Uri CallbackUri { get; }

    public JiraAuthProcess(Guid connectorId, string workerName, Uri callbackUri)
    {
        ConnectorId = connectorId;
        WorkerName = workerName;
        CallbackUri = callbackUri;
    }
}