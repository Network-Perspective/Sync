using System;

namespace NetworkPerspective.Sync.Framework.Dtos
{
    /// <summary>
    /// Single event log entry
    /// </summary>
    public class StatusLogDto
    {
        /// <summary>
        /// Timestamp of the event
        /// </summary>
        /// <example>2020-01-01T10:00:00</example>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// Message
        /// </summary>
        /// <example>Sync completed</example>
        public string Message { get; set; }
        /// <summary>
        /// Severity
        /// </summary>
        public StatusLogLevelDto Level { get; set; }
    }
}