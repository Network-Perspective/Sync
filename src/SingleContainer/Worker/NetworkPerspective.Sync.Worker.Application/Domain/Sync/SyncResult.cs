using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Sync;

public class SyncResult
{
    public readonly static SyncResult Empty = new(0, 0, []);

    public int TasksCount { get; }
    public int FailedTasksCount { get; }
    public long TotalInteractionsCount { get; }
    public double SuccessRate { get; }
    public IReadOnlyCollection<Exception> Exceptions { get; }

    public SyncResult(int tasksCount, long totalInteractionsCount, IEnumerable<Exception> exceptions)
    {
        TasksCount = tasksCount;
        FailedTasksCount = exceptions.Count();
        TotalInteractionsCount = totalInteractionsCount;
        SuccessRate = tasksCount != 0
            ? (tasksCount - FailedTasksCount) * 100.0 / tasksCount
            : 0.0;
        Exceptions = exceptions.ToList();
    }

    public static SyncResult Combine(params SyncResult[] syncResults)
    {
        var totalTasksCount = syncResults.Sum(x => x.TasksCount);
        var totalInteractionsCount = syncResults.Sum(x => x.TotalInteractionsCount);
        var allExceptions = syncResults.SelectMany(x => x.Exceptions);

        return new SyncResult(totalTasksCount, totalInteractionsCount, allExceptions);
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Tasks: {TasksCount}");
        stringBuilder.AppendLine($"Failed: {FailedTasksCount}");
        stringBuilder.AppendLine($"SuccessRate: {SuccessRate:0.00}%");
        stringBuilder.Append($"Total interactions: {TotalInteractionsCount}");

        return stringBuilder.ToString();
    }
}