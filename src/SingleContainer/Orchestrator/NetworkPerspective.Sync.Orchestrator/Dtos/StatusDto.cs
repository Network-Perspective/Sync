using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Dtos;

/// <summary>
/// Connector status
/// </summary>
public class StatusDto
{
    /// <summary>
    /// Indicates if worker responsible for processing the connector is connected
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Define if connector is authorized in Network Perspective Core and Data Source, available only if connector is connected
    /// </summary>
    /// <example>true</example>
    public bool Authorized { get; set; }

    /// <summary>
    /// Define if connector has active scheduler, available only if connector is connected
    /// </summary>
    /// <example>true</example>
    public bool Scheduled { get; set; }

    /// <summary>
    /// Define if synchronization is currently running, available only if connector is connected
    /// </summary>
    /// <example>false</example>
    public bool Running { get; set; }

    /// <summary>
    /// Current task status, available only if connector is connected
    /// </summary>
    public SynchronizationTaskStatusDto CurrentTask { get; set; }

    /// <summary>
    /// List of recent logs
    /// </summary>
    public IEnumerable<StatusLogDto> Logs { get; set; }
}