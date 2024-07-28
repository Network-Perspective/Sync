using System;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos
{
    /// <summary>
    /// Scheduler start request
    /// </summary>
    public class SchedulerStartDto
    {
        /// <summary>
        /// Overrides when synchronization should start (optional)
        /// </summary>
        public DateTime? OverrideSyncPeriodStart { get; set; }
    }
}