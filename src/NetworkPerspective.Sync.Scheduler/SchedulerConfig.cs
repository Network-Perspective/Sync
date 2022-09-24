namespace NetworkPerspective.Sync.Scheduler
{
    internal class SchedulerConfig
    {
        public string CronExpression { get; set; }
        public bool UsePersistentStore { get; set; } = true;
    }
}