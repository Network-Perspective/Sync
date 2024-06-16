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
        private readonly Mock<ISyncContextFactory> _syncContextFactory = new();
        private readonly Mock<IConnectorInfoProvider> _connectorInfoProvider = new();
        private readonly Mock<ISyncContextAccessor> _syncContextAccessor = new();
        private readonly Mock<ISyncService> _syncServiceMock = new();
        private readonly Mock<ISyncHistoryService> _syncHistoryService = new();
        private readonly ILogger<SyncJob> _logger = NullLogger<SyncJob>.Instance;
        public SyncJobTests()
        {
            _syncContextFactory.Reset();
            _syncServiceMock.Reset();
            _syncHistoryService.Reset();
        }

        [Fact]
        public async Task ShouldCatchExceptions()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var jobContextMock = CreateContext(connectorId);

            _syncServiceMock
                .Setup(x => x.SyncAsync(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var syncJob = new SyncJob(_syncServiceMock.Object, _connectorInfoProvider.Object, _syncContextFactory.Object, _syncContextAccessor.Object, _syncHistoryService.Object, new Clock(), _logger);

            // Act
            Func<Task> func = async () => await syncJob.Execute(jobContextMock);

            // Assert
            await func.Should().NotThrowAsync();
        }

        private static IJobExecutionContext CreateContext(Guid connectorId)
        {
            var jobDetails = new Mock<IJobDetail>();
            jobDetails
                .Setup(x => x.Key)
                .Returns(new JobKey(connectorId.ToString()));

            var jobContextMock = new Mock<IJobExecutionContext>();
            jobContextMock
                .Setup(x => x.JobDetail)
                .Returns(jobDetails.Object);

            return jobContextMock.Object;
        }
    }
}