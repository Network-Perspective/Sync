using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache;
using NetworkPerspective.Sync.Application.Scheduler;
using NetworkPerspective.Sync.Application.Services;

using Quartz;

using Xunit;

namespace NetworkPerspective.Sync.Scheduler.Tests
{
    public class SyncJobTests
    {
        [Fact]
        public async Task ShouldSyncEveryDaySeparetely()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var startTimeStamp = new DateTime(2021, 1, 1);
            var endTimeStamp = new DateTime(2021, 1, 3, 10, 0, 0);
            var syncHistory = new List<TimeRange>();

            var syncServiceFactoryMock = CreateSyncServiceFactoryMock(x => syncHistory.Add(x));
            var syncHistoryServiceMock = CreateSyncHistoryServiceMock(startTimeStamp);
            var networkServiceMock = CreateNetworkServiceMock(networkId, false);

            var clockMock = new Mock<IClock>();
            clockMock
                .SetupSequence(x => x.UtcNow())
                .Returns(endTimeStamp)
                .Returns(endTimeStamp.AddMinutes(1))
                .Returns(endTimeStamp.AddMinutes(2))
                .Returns(endTimeStamp.AddMinutes(3));

            var jobContextMock = CreateContext(networkId);

            var syncJob = new SyncJob(syncServiceFactoryMock, Mock.Of<INetworkPerspectiveCore>(), Mock.Of<ITokenService>(), syncHistoryServiceMock, networkServiceMock, clockMock.Object, Mock.Of<IInteractionsCacheFactory>(), Mock.Of<IStatusLogger>(), NullLogger<SyncJob>.Instance);

            // Act
            await syncJob.Execute(jobContextMock);

            // Assert
            var expectedTimeRanges = new[]
            {
                new TimeRange(startTimeStamp, new DateTime(2021, 1, 2)),
                new TimeRange(new DateTime(2021, 1, 2), new DateTime(2021, 1, 3)),
                new TimeRange(new DateTime(2021, 1, 3), endTimeStamp.AddMinutes(2))
            };

            syncHistory.Should().BeEquivalentTo(expectedTimeRanges);
        }

        [Fact]
        public async Task ShouldSyncNoFuture()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var startTimeStamp = new DateTime(2021, 1, 1);
            var endTimeStamp = new DateTime(2021, 1, 1, 10, 0, 0);
            var syncHistory = new List<TimeRange>();

            var syncServiceFactoryMock = CreateSyncServiceFactoryMock(x => syncHistory.Add(x));
            var syncHistoryServiceMock = CreateSyncHistoryServiceMock(startTimeStamp);
            var networkServiceMock = CreateNetworkServiceMock(networkId, false);

            var clockMock = new Mock<IClock>();
            clockMock
                .SetupSequence(x => x.UtcNow())
                .Returns(endTimeStamp)
                .Returns(endTimeStamp.AddMinutes(1))
                .Returns(endTimeStamp.AddMinutes(2))
                .Returns(endTimeStamp.AddMinutes(3));

            var jobContextMock = CreateContext(networkId);

            var syncJob = new SyncJob(syncServiceFactoryMock, Mock.Of<INetworkPerspectiveCore>(), Mock.Of<ITokenService>(), syncHistoryServiceMock, networkServiceMock, clockMock.Object, Mock.Of<IInteractionsCacheFactory>(), Mock.Of<IStatusLogger>(), NullLogger<SyncJob>.Instance);

            // Act
            await syncJob.Execute(jobContextMock);

            // Assert
            var expectedTimeRanges = new[]
            {
                new TimeRange(startTimeStamp, endTimeStamp)
            };

            syncHistory.Should().BeEquivalentTo(expectedTimeRanges);
        }

        [Fact]
        public async Task ShouldCatchExceptions()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var startTimeStamp = new DateTime(2021, 1, 1);
            var endTimeStamp = new DateTime(2021, 1, 1, 10, 0, 0);
            var syncHistory = new List<TimeRange>();

            var syncServiceFactoryMock = CreateSyncServiceFactoryMock(_ => throw new Exception());
            var syncHistoryServiceMock = CreateSyncHistoryServiceMock(startTimeStamp);
            var networkServiceMock = CreateNetworkServiceMock(networkId, false);
            var statusLoggerMock = new Mock<IStatusLogger>();

            var jobContextMock = CreateContext(networkId);

            var syncJob = new SyncJob(syncServiceFactoryMock, Mock.Of<INetworkPerspectiveCore>(), Mock.Of<ITokenService>(), syncHistoryServiceMock, networkServiceMock, new Clock(), Mock.Of<IInteractionsCacheFactory>(), statusLoggerMock.Object, NullLogger<SyncJob>.Instance);

            // Act
            Func<Task> func = async () => await syncJob.Execute(jobContextMock);

            // Assert
            await func.Should().NotThrowAsync();
            statusLoggerMock.Verify(x => x.AddLogAsync(It.Is<StatusLog>(l => l.Level == StatusLogLevel.Error), It.IsAny<CancellationToken>()), Times.Once);
        }

        private static IJobExecutionContext CreateContext(Guid networkId)
        {
            var jobDetails = new Mock<IJobDetail>();
            jobDetails
                .Setup(x => x.Key)
                .Returns(new JobKey(networkId.ToString()));

            var jobContextMock = new Mock<IJobExecutionContext>();
            jobContextMock
                .Setup(x => x.JobDetail)
                .Returns(jobDetails.Object);

            return jobContextMock.Object;
        }

        private INetworkService CreateNetworkServiceMock(Guid networkId, bool syncGroups)
        {
            var networkServiceMock = new Mock<INetworkService>();

            networkServiceMock
                .Setup(x => x.GetAsync<NetworkProperties>(networkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Network<NetworkProperties>.Create(networkId, new NetworkProperties(syncGroups, null, false), DateTime.UtcNow));

            return networkServiceMock.Object;
        }

        private ISyncHistoryService CreateSyncHistoryServiceMock(DateTime lastSync)
        {
            var syncHistoryServiceMock = new Mock<ISyncHistoryService>();
            syncHistoryServiceMock
                .Setup(x => x.EvaluateSyncStartAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastSync);

            return syncHistoryServiceMock.Object;
        }

        private ISyncServiceFactory CreateSyncServiceFactoryMock(Action<TimeRange> syncCallback)
        {
            var syncServiceMock = new Mock<ISyncService>();
            syncServiceMock
                .Setup(x => x.SyncInteractionsAsync(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
                .Callback<SyncContext, CancellationToken>((x, _) => syncCallback(x.CurrentRange));

            var factoryMock = new Mock<ISyncServiceFactory>();
            factoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(syncServiceMock.Object);

            return factoryMock.Object;
        }
    }
}