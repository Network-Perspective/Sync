using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Quartz;

namespace NetworkPerspective.Sync.Scheduler.Tests
{
    public class TestableJob : IJob
    {
        public const int JobExcecutionTimeInMs = 1000;
        private static readonly IList<Guid> InternalExecutedJobs = new List<Guid>();

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        public static IReadOnlyCollection<Guid> ExecutedJobs
        {
            get
            {
                try
                {
                    Semaphore.Wait();
                    return new ReadOnlyCollection<Guid>(InternalExecutedJobs.ToList());
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
                InternalExecutedJobs.Add(Guid.Parse(context.JobDetail.Key.Name));
            }
            finally
            {
                Semaphore.Release();
            }
            await Task.Delay(JobExcecutionTimeInMs, context.CancellationToken);
        }
    }
}