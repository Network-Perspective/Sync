using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        public async Task AddOrReplaceAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var jobKey = CreateJobKey(connectorId);
            var triggerKey = CreateTriggerKey(connectorId);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            if (await scheduler.GetJobDetail(jobKey, stoppingToken) != null)
            {
                _logger.LogInformation("There is already a schedule for Connector '{connectorId}', removing the schedule...", connectorId);
                await EnsureRemovedAsync(connectorId, stoppingToken);
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

            _logger.LogInformation("Connector '{connectorId}' schedule added", connectorId);
        }

        public async Task EnsureRemovedAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(connectorId);

            var isDeleted = await scheduler.DeleteJob(jobKey, stoppingToken);

            if (isDeleted)
                _logger.LogDebug("Connector '{connectorId}' schedule removed", connectorId);
            else
                _logger.LogDebug("Nothing to remove... schedule for Connector '{connectorId}' doesnt exist", connectorId);
        }

        public async Task TriggerNowAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Triggering manually job for Connector '{connectorId}'", connectorId);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            await scheduler.TriggerJob(CreateJobKey(connectorId), stoppingToken);

            _logger.LogDebug("Connector '{connectorId}' triggered manually", connectorId);
        }

        public async Task InterruptNowAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Interrupting current job for Connector '{connectorId}'", connectorId);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            await scheduler.Interrupt(CreateJobKey(connectorId), stoppingToken);

            _logger.LogDebug("Connector '{connectorId}' interrupted", connectorId);
        }

        public async Task ScheduleAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var triggerKey = CreateTriggerKey(connectorId);

            await scheduler.ResumeTrigger(triggerKey, stoppingToken);

            var nextExecutionTime = (await scheduler.GetTrigger(triggerKey)).GetNextFireTimeUtc();
            _logger.LogInformation("Connector '{connectorId}' scheduled. Next execution at '{executionTime}'", connectorId, nextExecutionTime);
        }

        public async Task UnscheduleAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var triggers = await scheduler.GetTriggersOfJob(CreateJobKey(connectorId));

            foreach (var trigger in triggers)
                await scheduler.PauseTrigger(trigger.Key);

            _logger.LogInformation("Connector '{connectorId}' unscheduled", connectorId);
        }

        public async Task<bool> IsScheduledAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(connectorId);
            var job = await scheduler.GetJobDetail(jobKey, stoppingToken);

            if (job is null)
                return false;

            var triggerKey = CreateTriggerKey(connectorId);

            var triggerState = await scheduler.GetTriggerState(triggerKey, stoppingToken);

            return triggerState != TriggerState.Paused && triggerState != TriggerState.None;
        }

        public async Task<bool> IsRunningAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(connectorId);
            var executingJobs = await scheduler.GetCurrentlyExecutingJobs(stoppingToken);

            return executingJobs.Any(x => AreJobKeysEqual(x.JobDetail.Key, jobKey));
        }

        private static TriggerKey CreateTriggerKey(Guid connectorId)
            => new TriggerKey(connectorId.ToString());

        private static JobKey CreateJobKey(Guid connectorId)
            => new JobKey(connectorId.ToString());

        private static bool AreJobKeysEqual(JobKey a, JobKey b)
            => a.Group == b.Group && a.Name == b.Name;
    }
}