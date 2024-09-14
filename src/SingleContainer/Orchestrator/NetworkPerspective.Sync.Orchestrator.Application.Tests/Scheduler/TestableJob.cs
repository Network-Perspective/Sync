using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Scheduler;

public class TestableJob : IJob
{
    public const int JobExcecutionTimeInMs = 1000;
    private static readonly IList<string> InternalExecutedJobs = [];
    private static readonly IList<string> InternalCompletedJobs = [];

    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public static IReadOnlyCollection<string> ExecutedJobs
    {
        get
        {
            try
            {
                Semaphore.Wait();
                return new ReadOnlyCollection<string>(InternalExecutedJobs.ToList());
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }

    public static IReadOnlyCollection<string> CompletedJobs
    {
        get
        {
            try
            {
                Semaphore.Wait();
                return new ReadOnlyCollection<string>(InternalCompletedJobs.ToList());
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }

    public static void Reset()
    {
        try
        {
            Semaphore.Wait();
            InternalExecutedJobs.Clear();
            InternalCompletedJobs.Clear();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await Semaphore.WaitAsync();
            InternalExecutedJobs.Add(context.JobDetail.Key.Name);
        }
        finally
        {
            Semaphore.Release();
        }

        await Task.Delay(JobExcecutionTimeInMs, context.CancellationToken);

        try
        {
            await Semaphore.WaitAsync();
            InternalCompletedJobs.Add(context.JobDetail.Key.Name);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}