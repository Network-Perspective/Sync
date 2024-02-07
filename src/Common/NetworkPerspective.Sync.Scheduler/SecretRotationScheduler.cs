using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Services;

using Quartz;

using IQuartzSchedulerFactory = Quartz.ISchedulerFactory;

namespace NetworkPerspective.Sync.Scheduler;

public class SecretRotationConst
{
    public const string SecretRotationJobKey = "SecretsRotationJob";
    public const string SecretRotationTriggerKey = "SecretsRotationTrigger";
}
public class SecretRotationScheduler : ISecretRotationScheduler
{
    private readonly IQuartzSchedulerFactory _schedulerFactory;
    private readonly SecretRotationConfig _config;
    private readonly ILogger<SecretRotationScheduler> _logger;

    public SecretRotationScheduler(IQuartzSchedulerFactory schedulerFactory, IOptions<SecretRotationConfig> config, ILogger<SecretRotationScheduler> logger)
    {
        _schedulerFactory = schedulerFactory;
        _config = config.Value;
        _logger = logger;
    }

    public async Task ScheduleSecretsRotation()
    {
        var jobKey = new JobKey(SecretRotationConst.SecretRotationJobKey);
        var triggerKey = new TriggerKey(SecretRotationConst.SecretRotationTriggerKey);

        var scheduler = await _schedulerFactory.GetScheduler();

        // is the job already scheduled?
        var existingJob = await scheduler.GetJobDetail(jobKey);
        if (existingJob != null)
        {
            _logger.LogInformation("Removing existing secret rotation job.");
            await scheduler.DeleteJob(jobKey);
        }

        if (!_config.Enabled)
        {
            return;
        }

        _logger.LogInformation("Scheduling secret rotation job...");

        var job = JobBuilder.Create<SecretRotationJob>()
            .WithIdentity(jobKey)
            .StoreDurably()
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity(triggerKey)
            .StartNow()
            .WithCronSchedule(_config.CronExpression)
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        _logger.LogInformation("Secret rotation job scheduled");

        if (_config.RotateOnStartup)
        {
            _logger.LogInformation("Triggering secret rotation on startup");
            await scheduler.TriggerJob(jobKey);
        }
    }
}