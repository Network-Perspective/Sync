using System;

namespace NetworkPerspective.Sync.Orchestrator.MicrosoftAuth;

public class MicrosoftAuthProcess
{
    public Guid ConnectorId { get; }
    public string WorkerName { get; }
    public Uri CallbackUri { get; }
    public bool SyncMsTeams { get; }

    public MicrosoftAuthProcess(Guid connectorId, string workerName, Uri callbackUri, bool syncMsTeams)
    {
        ConnectorId = connectorId;
        WorkerName = workerName;
        CallbackUri = callbackUri;
        SyncMsTeams = syncMsTeams;
    }
}