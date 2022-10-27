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

        public async Task AddOrReplaceAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var jobKey = CreateJobKey(networkId);
            var triggerKey = CreateTriggerKey(networkId);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            if (await scheduler.GetJobDetail(jobKey, stoppingToken) != null)
            {
                _logger.LogInformation("There is already a schedule for Network '{networkId}', removing the schedule...", networkId);
                await EnsureRemovedAsync(networkId, stoppingToken);
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

            _logger.LogInformation("Network '{networkID}' schedule added", networkId);
        }

        public async Task EnsureRemovedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(networkId);

            var isDeleted = await scheduler.DeleteJob(jobKey, stoppingToken);

            if (isDeleted)
                _logger.LogDebug("Network '{0}' schedule removed", networkId);
            else
                _logger.LogDebug("Nothing to remove... schedule for Network '{0}' doesnt exist", networkId);
        }

        public async Task TriggerNowAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Triggering manually job for Network '{networkId}'", networkId);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            await scheduler.TriggerJob(CreateJobKey(networkId), stoppingToken);

            _logger.LogDebug("Network '{networkId}' triggered manually", networkId);
        }

        public async Task InterruptNowAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Interrupting current job for Network '{networkId}'", networkId);

            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            await scheduler.Interrupt(CreateJobKey(networkId), stoppingToken);

            _logger.LogDebug("Network '{networkId}' interrupted", networkId);
        }

        public async Task ScheduleAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var triggerKey = CreateTriggerKey(networkId);

            await scheduler.ResumeTrigger(triggerKey, stoppingToken);

            var nextExecutionTime = (await scheduler.GetTrigger(triggerKey)).GetNextFireTimeUtc();
            _logger.LogInformation("Network '{networkId}' scheduled. Next execution at '{executionTime}'", networkId, nextExecutionTime);
        }

        public async Task UnscheduleAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var triggers = await scheduler.GetTriggersOfJob(CreateJobKey(networkId));

            foreach (var trigger in triggers)
                await scheduler.PauseTrigger(trigger.Key);

            _logger.LogInformation("Network '{networkId}' unscheduled", networkId);
        }

        public async Task<bool> IsScheduledAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(networkId);
            var job = await scheduler.GetJobDetail(jobKey, stoppingToken);

            if (job is null)
                return false;

            var triggerKey = CreateTriggerKey(networkId);

            var triggerState = await scheduler.GetTriggerState(triggerKey, stoppingToken);

            return triggerState != TriggerState.Paused && triggerState != TriggerState.None;
        }

        public async Task<bool> IsRunningAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);

            var jobKey = CreateJobKey(networkId);
            var executingJobs = await scheduler.GetCurrentlyExecutingJobs(stoppingToken);

            return executingJobs.Any(x => AreJobKeysEqual(x.JobDetail.Key, jobKey));
        }

        private static TriggerKey CreateTriggerKey(Guid networkId)
            => new TriggerKey(networkId.ToString());

        private static JobKey CreateJobKey(Guid networkId)
            => new JobKey(networkId.ToString());

        private static bool AreJobKeysEqual(JobKey a, JobKey b)
            => a.Group == b.Group && a.Name == b.Name;
    }
}