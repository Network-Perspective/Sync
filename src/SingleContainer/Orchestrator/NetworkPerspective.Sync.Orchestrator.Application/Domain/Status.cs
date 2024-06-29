using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class Status
{
    public bool IsConnected { get; set; }
    public bool Authorized { get; set; }
    public bool Scheduled { get; set; }
    public bool Running { get; set; }
    public SingleTaskStatus CurrentTask { get; set; }
    public IEnumerable<StatusLog> Logs { get; set; }
}