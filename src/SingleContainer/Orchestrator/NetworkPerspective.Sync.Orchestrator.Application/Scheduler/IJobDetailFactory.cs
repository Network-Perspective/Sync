using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler;

public interface IJobDetailFactory
{
    IJobDetail Create(JobKey jobKey);
}

internal class JobDetailFactory<TJob> : IJobDetailFactory where TJob : class, IJob
{
    public IJobDetail Create(JobKey jobKey)
    {
        return JobBuilder.Create<TJob>()
            .WithIdentity(jobKey)
            .StoreDurably()
            .Build();
    }
}