using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Scheduler;
using NetworkPerspective.Sync.Application.Services;

using Quartz;

using Xunit;

namespace NetworkPerspective.Sync.Scheduler.Tests
{
    public class SyncJobTests
    {
        private readonly Mock<ISyncContextFactory> _syncContextFactory = new Mock<ISyncContextFactory>();
        private readonly Mock<ISyncServiceFactory> _syncServiceFactoryMock = new Mock<ISyncServiceFactory>();
        private readonly Mock<ISyncService> _syncServiceMock = new Mock<ISyncService>();
        private readonly ILogger<SyncJob> _logger = NullLogger<SyncJob>.Instance;
        public SyncJobTests()
        {
            _syncContextFactory.Reset();
            _syncServiceFactoryMock.Reset();
            _syncServiceMock.Reset();

            _syncServiceFactoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_syncServiceMock.Object);
        }

        [Fact]
        public async Task ShouldCatchExceptions()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var jobContextMock = CreateContext(networkId);

            _syncServiceMock
                .Setup(x => x.SyncAsync(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var syncJob = new SyncJob(_syncContextFactory.Object, _syncServiceFactoryMock.Object, _logger);

            // Act
            Func<Task> func = async () => await syncJob.Execute(jobContextMock);

            // Assert
            await func.Should().NotThrowAsync();
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
    }
}