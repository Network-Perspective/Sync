using System;

namespace NetworkPerspective.Sync.Framework.Dtos
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