using System.Collections.Generic;

namespace NetworkPerspective.Sync.Framework.Dtos
{
    /// <summary>
    /// Network status
    /// </summary>
    public class StatusDto
    {
        /// <summary>
        /// Define if network is authorized in Network Perspective Core and Data Source
        /// </summary>
        /// <example>true</example>
        public bool Authorized { get; set; }
        /// <summary>
        /// Define if network has active scheduler
        /// </summary>
        /// <example>true</example>
        public bool Scheduled { get; set; }
        /// <summary>
        /// Define if synchronization is currently running
        /// </summary>
        /// <example>false</example>
        public bool Running { get; set; }
        /// <summary>
        /// Current task status
        /// </summary>
        public TaskStatusDto CurrentTask { get; set; }
        /// <summary>
        /// List of recent logs
        /// </summary>
        public IEnumerable<StatusLogDto> Logs { get; set; }
    }
}