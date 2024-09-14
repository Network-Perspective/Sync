using System;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class SyncCompletedDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Guid ConnectorId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public double SuccessRate { get; set; }
    public long TotalInteractionsCount { get; set; }
    public int TasksCount { get; set; }
    public int FailedTasksCount { get; set; }
}