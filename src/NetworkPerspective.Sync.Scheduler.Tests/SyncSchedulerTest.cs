using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

using Xunit;

namespace NetworkPerspective.Sync.Scheduler.Tests
{
    [Collection("Sequential")]
    public class SyncSchedulerTest
    {
        private const int TimeoutInMs = TestableJob.JobExcecutionTimeInMs * 2;
        private readonly ISchedulerFactory _schedulerFactory = new StdSchedulerFactory();
        private readonly IScheduler _scheduler;

        private readonly IJobDetailFactory _jobDetailFactory = new JobDetailFactory<TestableJob>();
        private readonly ILogger<SyncScheduler> _logger = NullLogger<SyncScheduler>.Instance;
        private readonly IOptions<SchedulerConfig> _defaultOptions = CreateOptions();

        public SyncSchedulerTest()
        {
            TestableJob.Reset();

            _scheduler = _schedulerFactory.GetScheduler().Result;
            _scheduler.DeleteJobs(_scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).Result).Wait();
            _scheduler.Standby();
        }

        public class Add : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldReplaceJobIfAlreadyExist()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);

                // Act
                await syncScheduler.AddOrReplaceAsync(networkId);

                // Assert
                var alljobs = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                alljobs.Should().BeEquivalentTo(new[] { new JobKey(networkId.ToString()) });
            }
        }

        public class Remove : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldRemoveJob()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);

                // Act
                await syncScheduler.EnsureRemovedAsync(networkId);

                // Assert
                var alljobs = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                alljobs.Should().BeEmpty();
            }

            [Fact]
            public async Task ShouldNotThrowOnNotExisting()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);

                Func<Task> func = async () => await syncScheduler.EnsureRemovedAsync(networkId);

                // Act Assert
                await func.Should().NotThrowAsync();
            }
        }


        public class TriggerNow : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldTriggerWithCorrectNetworkId()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);

                // Act
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(networkId);

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == networkId), TimeoutInMs).Should().BeTrue();
            }
        }

        public class InterruptNow : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldInterruptWithCorrectNetworkId()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(networkId);
                SpinWait.SpinUntil(() => syncScheduler.IsRunningAsync(networkId).Result, TimeoutInMs);

                // Act
                await syncScheduler.InterruptNowAsync(networkId);

                // Assert
                var interruptionTimeout = TestableJob.JobExcecutionTimeInMs / 5;
                SpinWait.SpinUntil(() => !syncScheduler.IsRunningAsync(networkId).Result, interruptionTimeout).Should().BeTrue();
            }
        }

        public class Schedule : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldTriggerWithCorrectNetworkId()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);

                // Act
                await syncScheduler.ScheduleAsync(networkId);
                await _scheduler.Start();

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == networkId), TimeoutInMs).Should().BeTrue();
            }
        }

        public class Unschedule : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldUnscheduleAllPendingJobs()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);
                await syncScheduler.ScheduleAsync(networkId);
                await syncScheduler.TriggerNowAsync(networkId);

                // Act
                await syncScheduler.UnscheduleAsync(networkId);
                await _scheduler.Start();

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == networkId), TimeoutInMs).Should().BeFalse();
            }
        }

        public class IsRunning : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldReturnTrueForRunningJob()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(networkId);

                // Act Assert
                SpinWait.SpinUntil(() => syncScheduler.IsRunningAsync(networkId).Result, TimeoutInMs).Should().BeTrue();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotRunningJob()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);

                // Act
                var result = await syncScheduler.IsRunningAsync(networkId);

                // Assert
                result.Should().BeFalse();
            }
        }


        public class IsScheduled : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldReturnFalseForNotAddedJob()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);

                // Act
                var result = await syncScheduler.IsScheduledAsync(networkId);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotAddedTrigger()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);
                var quartzScheduler = await _schedulerFactory.GetScheduler();
                await quartzScheduler.UnscheduleJob(new TriggerKey(networkId.ToString()));

                // Act
                var result = await syncScheduler.IsScheduledAsync(networkId);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotScheduledJob()
            {
                // Arrange
                var networkId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);
                await syncScheduler.ScheduleAsync(networkId);
                await syncScheduler.UnscheduleAsync(networkId);

                // Act
                var result = await syncScheduler.IsScheduledAsync(networkId);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnTrueForScheduledJobs()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(networkId);
                await syncScheduler.ScheduleAsync(networkId);

                // Act
                var result = await syncScheduler.IsScheduledAsync(networkId);

                // Assert
                result.Should().BeTrue();
            }
        }

        private static IOptions<SchedulerConfig> CreateOptions(string cronExpression = "0/1 * * * * ?")
            => Options.Create(new SchedulerConfig { CronExpression = cronExpression });

    }
}