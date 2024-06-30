namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;

internal class SyncSchedulerConfig
{
    public string CronExpression { get; set; }
    public bool UsePersistentStore { get; set; } = true;
}