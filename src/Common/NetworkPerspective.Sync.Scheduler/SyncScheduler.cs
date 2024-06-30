using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Services;

using Quartz;

using IQuartzSchedulerFactory = Quartz.ISchedulerFactory;

namespace NetworkPerspective.Sync.Scheduler
{
    internal class SyncScheduler : ISyncScheduler
    {
        private readonly IQuartzSchedulerFactory _schedulerFactory;
        private readonly IJobDetailFactory _jobDetailFactory;
        private readonly SchedulerConfig _config;
        private readonly ILogger<SyncScheduler> _logger;

        public SyncScheduler(IQuartzSchedulerFactory schedulerFactory, IJobDetailFactory jobDetailFactory, IOptions<SchedulerConfig> config, ILogger<SyncScheduler> logger)
        {
            _schedulerFactory = schedulerFactory;
            _jobDetailFactory = jobDetailFactory;
            _config = config.Value;
            _logger = logger;
        }

        public async Task AddOrReplaceAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            var jobKey = CreateJobKey(connectorInfo);
            var triggerKey = CreateTriggerKey(connectorInfo);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            if (await scheduler.GetJobDetail(jobKey, stoppingToken) != null)
            {
                _logger.LogInformation("There is already a schedule for Connector '{connectorId}', removing the schedule...", connectorInfo.Id);
                await EnsureRemovedAsync(connectorInfo, stoppingToken);
            }

            var job = _jobDetailFactory.Create(jobKey);

            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(triggerKey)
                .StartNow()
                .WithCronSchedule(_config.CronExpression)
                .Build();

            await scheduler.ScheduleJob(job, trigger, stoppingToken);
            await scheduler.PauseTrigger(triggerKey);

            _logger.LogInformation("Connector '{connectorId}' schedule added", connectorInfo.Id);
        }

        public async Task EnsureRemovedAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(connectorInfo);

            var isDeleted = await scheduler.DeleteJob(jobKey, stoppingToken);

            if (isDeleted)
                _logger.LogDebug("Connector '{connectorId}' schedule removed", connectorInfo.Id);
            else
                _logger.LogDebug("Nothing to remove... schedule for Connector '{connectorId}' doesnt exist", connectorInfo.Id);
        }

        public async Task TriggerNowAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Triggering manually job for Connector '{connectorId}'", connectorInfo.Id);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            await scheduler.TriggerJob(CreateJobKey(connectorInfo), stoppingToken);

            _logger.LogDebug("Connector '{connectorId}' triggered manually", connectorInfo.Id);
        }

        public async Task InterruptNowAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Interrupting current job for Connector '{connectorId}'", connectorInfo.Id);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            await scheduler.Interrupt(CreateJobKey(connectorInfo), stoppingToken);

            _logger.LogDebug("Connector '{connectorId}' interrupted", connectorInfo.Id);
        }

        public async Task ScheduleAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var triggerKey = CreateTriggerKey(connectorInfo);

            await scheduler.ResumeTrigger(triggerKey, stoppingToken);

            var nextExecutionTime = (await scheduler.GetTrigger(triggerKey)).GetNextFireTimeUtc();
            _logger.LogInformation("Connector '{connectorId}' scheduled. Next execution at '{executionTime}'", connectorInfo.Id, nextExecutionTime);
        }

        public async Task UnscheduleAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var triggers = await scheduler.GetTriggersOfJob(CreateJobKey(connectorInfo));

            foreach (var trigger in triggers)
                await scheduler.PauseTrigger(trigger.Key);

            _logger.LogInformation("Connector '{connectorId}' unscheduled", connectorInfo.Id);
        }

        public async Task<bool> IsScheduledAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(connectorInfo);
            var job = await scheduler.GetJobDetail(jobKey, stoppingToken);

            if (job is null)
                return false;

            var triggerKey = CreateTriggerKey(connectorInfo);

            var triggerState = await scheduler.GetTriggerState(triggerKey, stoppingToken);

            return triggerState != TriggerState.Paused && triggerState != TriggerState.None;
        }

        public async Task<bool> IsRunningAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(connectorInfo);
            var executingJobs = await scheduler.GetCurrentlyExecutingJobs(stoppingToken);

            return executingJobs.Any(x => AreJobKeysEqual(x.JobDetail.Key, jobKey));
        }

        private static TriggerKey CreateTriggerKey(ConnectorInfo connectorInfo)
            => new TriggerKey($"{connectorInfo.Id.ToString()}:{connectorInfo.NetworkId.ToString()}");

        private static JobKey CreateJobKey(ConnectorInfo connectorInfo)
            => new JobKey($"{connectorInfo.Id.ToString()}:{connectorInfo.NetworkId.ToString()}");

        private static bool AreJobKeysEqual(JobKey a, JobKey b)
            => a.Group == b.Group && a.Name == b.Name;
    }
}