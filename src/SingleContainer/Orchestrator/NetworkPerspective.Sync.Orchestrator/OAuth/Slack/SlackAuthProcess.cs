using System;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Slack;

public class SlackAuthProcess
{
    public Guid ConnectorId { get; }
    public string WorkerName { get; }
    public Uri CallbackUri { get; }
    public bool RequireAdminPrivileges { get; }

    public SlackAuthProcess(Guid connectorId, string workerName, Uri callbackUri, bool requireAdminPrivileges)
    {
        ConnectorId = connectorId;
        WorkerName = workerName;
        CallbackUri = callbackUri;
        RequireAdminPrivileges = requireAdminPrivileges;
    }
}