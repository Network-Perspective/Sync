namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

/// <summary>
/// Single synchronization task status
/// </summary>
public class SynchronizationTaskStatusDto
{
    /// <summary>
    /// Task Caption
    /// </summary>
    /// <example>Synchronizing entities</example>
    public string Caption { get; set; }
    /// <summary>
    /// Task description
    /// </summary>
    /// <example>Fetching users data from Google API</example>
    public string Description { get; set; }
    /// <summary>
    /// Completion rate [0-100]% 
    /// </summary>
    /// <example>33.4</example>
    public double? CompletionRate { get; set; }
}