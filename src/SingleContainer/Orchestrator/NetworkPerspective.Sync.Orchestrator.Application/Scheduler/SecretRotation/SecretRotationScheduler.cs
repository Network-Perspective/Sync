using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using IQuartzSchedulerFactory = Quartz.ISchedulerFactory;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.SecretRotation;

internal class SecretRotationScheduler : BackgroundService
{
    public const string SecretRotationJobKey = "SecretsRotationJob";
    public const string SecretRotationTriggerKey = "SecretsRotationTrigger";

    private readonly IQuartzSchedulerFactory _schedulerFactory;
    private readonly SecretRotationSchedulerConfig _config;
    private readonly ILogger<SecretRotationScheduler> _logger;

    public SecretRotationScheduler(IQuartzSchedulerFactory schedulerFactory, IOptions<SecretRotationSchedulerConfig> config, ILogger<SecretRotationScheduler> logger)
    {
        _schedulerFactory = schedulerFactory;
        _config = config.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //await Task.Delay(TimeSpan.FromMinutes(5));
        var jobKey = new JobKey(SecretRotationJobKey);
        var triggerKey = new TriggerKey(SecretRotationTriggerKey);

        var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

        // is the job already scheduled?
        var existingJob = await scheduler.GetJobDetail(jobKey, stoppingToken);
        if (existingJob != null)
        {
            _logger.LogInformation("Removing existing secret rotation job.");
            await scheduler.DeleteJob(jobKey);
        }

        if (!_config.Enabled)
            return;

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

        await scheduler.ScheduleJob(job, trigger, stoppingToken);

        _logger.LogInformation("Secret rotation job scheduled");

        if (_config.RotateOnStartup)
        {
            _logger.LogInformation("Triggering secret rotation on startup");
            await scheduler.TriggerJob(jobKey, stoppingToken);
        }
    }
}