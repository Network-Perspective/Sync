using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler;

internal class SchedulerConfig
{
    public bool UsePersistentStore { get; set; }
    public TimeSpan StartDelay { get; set; }
}