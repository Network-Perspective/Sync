using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Quartz;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Scheduler;

public class SyncJobTests
{
    private readonly Mock<IConnectorsService> _connectorsServiceMock = new();
    private readonly Mock<IWorkerRouter> _workerRouterMock = new();
    private readonly Mock<ISyncHistoryService> _syncHistoryServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly ILogger<RemoteSyncJob> _logger = NullLogger<RemoteSyncJob>.Instance;

    public SyncJobTests()
    {
        _connectorsServiceMock.Reset();
        _workerRouterMock.Reset();
        _syncHistoryServiceMock.Reset();
        _tokenServiceMock.Reset();
    }

    [Fact]
    public async Task ShouldCatchExceptions()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var jobContextMock = CreateContext(connectorId);

        _connectorsServiceMock
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception());

        var syncJob = new RemoteSyncJob(_connectorsServiceMock.Object, _workerRouterMock.Object, _syncHistoryServiceMock.Object, _tokenServiceMock.Object, new Clock(), _logger);

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