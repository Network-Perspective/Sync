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
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);

                // Act
                await syncScheduler.AddOrReplaceAsync(connectorId);

                // Assert
                var alljobs = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                alljobs.Should().BeEquivalentTo(new[] { new JobKey(connectorId.ToString()) });
            }
        }

        public class Remove : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldRemoveJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);

                // Act
                await syncScheduler.EnsureRemovedAsync(connectorId);

                // Assert
                var alljobs = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                alljobs.Should().BeEmpty();
            }

            [Fact]
            public async Task ShouldNotThrowOnNotExisting()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);

                Func<Task> func = async () => await syncScheduler.EnsureRemovedAsync(connectorId);

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
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);

                // Act
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(connectorId);

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == connectorId), TimeoutInMs).Should().BeTrue();
            }
        }

        public class InterruptNow : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldInterruptCurrentJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(connectorId);
                SpinWait.SpinUntil(() => syncScheduler.IsRunningAsync(connectorId).Result, TimeoutInMs);

                // Act
                await syncScheduler.InterruptNowAsync(connectorId);

                // Assert
                var interruptionTimeout = TestableJob.JobExcecutionTimeInMs / 5;
                SpinWait.SpinUntil(() => !syncScheduler.IsRunningAsync(connectorId).Result, interruptionTimeout).Should().BeTrue();
            }

            [Fact]
            public async Task ShouldNotInterfereWithOtherNetworksSync()
            {
                // Arrange
                var connectorId_1 = Guid.NewGuid();
                var connectorId_2 = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId_1);
                await syncScheduler.AddOrReplaceAsync(connectorId_2);
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(connectorId_2);

                // Act
                await syncScheduler.InterruptNowAsync(connectorId_1);

                // Assert
                SpinWait.SpinUntil(() => TestableJob.CompletedJobs.Contains(connectorId_2), TimeoutInMs).Should().BeTrue();
            }
        }

        public class Schedule : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldTriggerWithCorrectNetworkId()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);

                // Act
                await syncScheduler.ScheduleAsync(connectorId);
                await _scheduler.Start();

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == connectorId), TimeoutInMs).Should().BeTrue();
            }
        }

        public class Unschedule : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldUnscheduleAllPendingJobs()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);
                await syncScheduler.ScheduleAsync(connectorId);
                await syncScheduler.TriggerNowAsync(connectorId);

                // Act
                await syncScheduler.UnscheduleAsync(connectorId);
                await _scheduler.Start();

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == connectorId), TimeoutInMs).Should().BeFalse();
            }

            [Fact]
            public async Task ShouldNotInterfereWithOtherNetworksSync()
            {
                // Arrange
                var connectorId_1 = Guid.NewGuid();
                var connectorId_2 = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId_1);
                await syncScheduler.AddOrReplaceAsync(connectorId_2);
                await syncScheduler.ScheduleAsync(connectorId_1);
                await syncScheduler.ScheduleAsync(connectorId_2);
                await syncScheduler.TriggerNowAsync(connectorId_1);
                await syncScheduler.UnscheduleAsync(connectorId_1);

                // Act
                await _scheduler.Start();

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == connectorId_2), TimeoutInMs).Should().BeTrue();
            }
        }

        public class IsRunning : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldReturnTrueForRunningJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(connectorId);

                // Act Assert
                SpinWait.SpinUntil(() => syncScheduler.IsRunningAsync(connectorId).Result, TimeoutInMs).Should().BeTrue();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotRunningJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);

                // Act
                var result = await syncScheduler.IsRunningAsync(connectorId);

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
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);

                // Act
                var result = await syncScheduler.IsScheduledAsync(connectorId);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotAddedTrigger()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);
                var quartzScheduler = await _schedulerFactory.GetScheduler();
                await quartzScheduler.UnscheduleJob(new TriggerKey(connectorId.ToString()));

                // Act
                var result = await syncScheduler.IsScheduledAsync(connectorId);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotScheduledJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);
                await syncScheduler.ScheduleAsync(connectorId);
                await syncScheduler.UnscheduleAsync(connectorId);

                // Act
                var result = await syncScheduler.IsScheduledAsync(connectorId);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnTrueForScheduledJobs()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorId);
                await syncScheduler.ScheduleAsync(connectorId);

                // Act
                var result = await syncScheduler.IsScheduledAsync(connectorId);

                // Assert
                result.Should().BeTrue();
            }
        }

        private static IOptions<SchedulerConfig> CreateOptions(string cronExpression = "0/1 * * * * ?")
            => Options.Create(new SchedulerConfig { CronExpression = cronExpression });

    }
}