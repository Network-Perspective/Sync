namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.SecretRotation;

internal class SecretRotationSchedulerConfig
{
    public bool Enabled { get; set; }
    public bool RotateOnStartup { get; set; }
    public string CronExpression { get; set; }
}