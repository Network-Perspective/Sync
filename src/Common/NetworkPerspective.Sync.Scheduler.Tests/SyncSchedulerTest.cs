using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Connectors;

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
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);

                // Act
                await syncScheduler.AddOrReplaceAsync(connectorInfo);

                // Assert
                var alljobs = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                alljobs.Should().BeEquivalentTo(new[] { new JobKey(connectorInfo.ToString()) });
            }
        }

        public class Remove : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldRemoveJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);

                // Act
                await syncScheduler.EnsureRemovedAsync(connectorInfo);

                // Assert
                var alljobs = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                alljobs.Should().BeEmpty();
            }

            [Fact]
            public async Task ShouldNotThrowOnNotExisting()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);

                Func<Task> func = async () => await syncScheduler.EnsureRemovedAsync(connectorInfo);

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
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);

                // Act
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(connectorInfo);

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == connectorInfo.ToString()), TimeoutInMs).Should().BeTrue();
            }
        }

        public class InterruptNow : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldInterruptCurrentJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(connectorInfo);
                SpinWait.SpinUntil(() => syncScheduler.IsRunningAsync(connectorInfo).Result, TimeoutInMs);

                // Act
                await syncScheduler.InterruptNowAsync(connectorInfo);

                // Assert
                var interruptionTimeout = TestableJob.JobExcecutionTimeInMs / 5;
                SpinWait.SpinUntil(() => !syncScheduler.IsRunningAsync(connectorInfo).Result, interruptionTimeout).Should().BeTrue();
            }

            [Fact]
            public async Task ShouldNotInterfereWithOtherNetworksSync()
            {
                // Arrange
                var connectorId_1 = Guid.NewGuid();
                var networkId_1 = Guid.NewGuid();
                var connectorInfo_1 = new ConnectorInfo(connectorId_1, networkId_1);

                var connectorId_2 = Guid.NewGuid();
                var networkId_2 = Guid.NewGuid();
                var connectorInfo_2 = new ConnectorInfo(connectorId_2, networkId_2);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo_1);
                await syncScheduler.AddOrReplaceAsync(connectorInfo_2);
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(connectorInfo_2);

                // Act
                await syncScheduler.InterruptNowAsync(connectorInfo_1);

                // Assert
                SpinWait.SpinUntil(() => TestableJob.CompletedJobs.Contains(connectorInfo_2.ToString()), TimeoutInMs).Should().BeTrue();
            }
        }

        public class Schedule : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldTriggerWithCorrectNetworkId()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);

                // Act
                await syncScheduler.ScheduleAsync(connectorInfo);
                await _scheduler.Start();

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == connectorInfo.ToString()), TimeoutInMs).Should().BeTrue();
            }
        }

        public class Unschedule : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldUnscheduleAllPendingJobs()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);
                await syncScheduler.ScheduleAsync(connectorInfo);
                await syncScheduler.TriggerNowAsync(connectorInfo);

                // Act
                await syncScheduler.UnscheduleAsync(connectorInfo);
                await _scheduler.Start();

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == connectorInfo.ToString()), TimeoutInMs).Should().BeFalse();
            }

            [Fact]
            public async Task ShouldNotInterfereWithOtherConnectorSync()
            {
                // Arrange
                var connectorId_1 = Guid.NewGuid();
                var networkId_1 = Guid.NewGuid();
                var connectorInfo_1 = new ConnectorInfo(connectorId_1, networkId_1);

                var connectorId_2 = Guid.NewGuid();
                var networkId_2 = Guid.NewGuid();
                var connectorInfo_2 = new ConnectorInfo(connectorId_2, networkId_2);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo_1);
                await syncScheduler.AddOrReplaceAsync(connectorInfo_2);
                await syncScheduler.ScheduleAsync(connectorInfo_1);
                await syncScheduler.ScheduleAsync(connectorInfo_2);
                await syncScheduler.TriggerNowAsync(connectorInfo_1);
                await syncScheduler.UnscheduleAsync(connectorInfo_1);

                // Act
                await _scheduler.Start();

                // Assert
                SpinWait.SpinUntil(() => TestableJob.ExecutedJobs.Any(x => x == connectorInfo_2.ToString()), TimeoutInMs).Should().BeTrue();
            }
        }

        public class IsRunning : SyncSchedulerTest
        {
            [Fact]
            public async Task ShouldReturnTrueForRunningJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);
                await _scheduler.Start();
                await syncScheduler.TriggerNowAsync(connectorInfo);

                // Act Assert
                SpinWait.SpinUntil(() => syncScheduler.IsRunningAsync(connectorInfo).Result, TimeoutInMs).Should().BeTrue();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotRunningJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);

                // Act
                var result = await syncScheduler.IsRunningAsync(connectorInfo);

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
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);

                // Act
                var result = await syncScheduler.IsScheduledAsync(connectorInfo);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotAddedTrigger()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);
                var quartzScheduler = await _schedulerFactory.GetScheduler();
                await quartzScheduler.UnscheduleJob(new TriggerKey(connectorId.ToString()));

                // Act
                var result = await syncScheduler.IsScheduledAsync(connectorInfo);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnFalseForNotScheduledJob()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);
                await syncScheduler.ScheduleAsync(connectorInfo);
                await syncScheduler.UnscheduleAsync(connectorInfo);

                // Act
                var result = await syncScheduler.IsScheduledAsync(connectorInfo);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public async Task ShouldReturnTrueForScheduledJobs()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var networkId = Guid.NewGuid();
                var connectorInfo = new ConnectorInfo(connectorId, networkId);

                var syncScheduler = new SyncScheduler(_schedulerFactory, _jobDetailFactory, _defaultOptions, _logger);
                await syncScheduler.AddOrReplaceAsync(connectorInfo);
                await syncScheduler.ScheduleAsync(connectorInfo);

                // Act
                var result = await syncScheduler.IsScheduledAsync(connectorInfo);

                // Assert
                result.Should().BeTrue();
            }
        }

        private static IOptions<SchedulerConfig> CreateOptions(string cronExpression = "0/1 * * * * ?")
            => Options.Create(new SchedulerConfig { CronExpression = cronExpression });

    }
}