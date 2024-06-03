namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler;

internal class SchedulerConfig
{
    public string CronExpression { get; set; }
    public bool UsePersistentStore { get; set; } = true;
}