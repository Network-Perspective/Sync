using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using IQuartzSchedulerFactory = Quartz.ISchedulerFactory;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.SecretRotation;

internal class SecretRotationScheduler(IQuartzSchedulerFactory schedulerFactory, IOptions<SecretRotationSchedulerConfig> config, ILogger<SecretRotationScheduler> logger) : BackgroundService
{
    public const string SecretRotationJobKey = "SecretsRotationJob";
    public const string SecretRotationTriggerKey = "SecretsRotationTrigger";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobKey = new JobKey(SecretRotationJobKey);
        var triggerKey = new TriggerKey(SecretRotationTriggerKey);

        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);

        // is the job already scheduled?
        var existingJob = await scheduler.GetJobDetail(jobKey, stoppingToken);
        if (existingJob != null)
        {
            logger.LogInformation("Removing existing secret rotation job.");
            await scheduler.DeleteJob(jobKey, stoppingToken);
        }

        if (!config.Value.Enabled)
            return;

        logger.LogInformation("Scheduling secret rotation job...");

        var job = JobBuilder.Create<SecretRotationJob>()
            .WithIdentity(jobKey)
            .StoreDurably()
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity(triggerKey)
            .StartNow()
            .WithCronSchedule(config.Value.CronExpression)
            .Build();

        await scheduler.ScheduleJob(job, trigger, stoppingToken);

        logger.LogInformation("Secret rotation job scheduled");

        if (config.Value.RotateOnStartup)
        {
            logger.LogInformation("Triggering secret rotation on startup");
            await scheduler.TriggerJob(jobKey, stoppingToken);
        }
    }
}