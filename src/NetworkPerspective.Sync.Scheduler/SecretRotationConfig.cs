namespace NetworkPerspective.Sync.Scheduler;

public class SecretRotationConfig
{
    public bool Enabled { get; set; }
    public string CronExpression { get; set; }
}