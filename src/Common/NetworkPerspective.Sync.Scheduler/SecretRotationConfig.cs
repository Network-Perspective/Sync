namespace NetworkPerspective.Sync.Scheduler;

public class SecretRotationConfig
{
    public bool Enabled { get; set; }
    public bool RotateOnStartup { get; set; }
    public string CronExpression { get; set; }
}