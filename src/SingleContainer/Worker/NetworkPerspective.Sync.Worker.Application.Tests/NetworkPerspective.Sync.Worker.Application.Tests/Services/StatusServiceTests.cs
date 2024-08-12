//using System;
//using System.Threading;
//using System.Threading.Tasks;

//using FluentAssertions;

//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Abstractions;

//using Moq;

//using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
//using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
//using NetworkPerspective.Sync.Worker.Application.Services;
//using NetworkPerspective.Sync.Common.Tests;

//using Xunit;

//namespace NetworkPerspective.Sync.Application.Tests.Services
//{
//    public class StatusServiceTests
//    {
//        private static readonly ILogger<StatusService> NullLogger = NullLogger<StatusService>.Instance;
//        private readonly Mock<ITokenService> _tokenServiceMock = new();
//        private readonly Mock<ISyncScheduler> _schedulerMock = new();
//        private readonly Mock<ITasksStatusesCache> _tasksStatusesCache = new();
//        private readonly Mock<IAuthTester> _authTesterMock = new();

//        public StatusServiceTests()
//        {
//            _tokenServiceMock.Reset();
//            _schedulerMock.Reset();
//            _tasksStatusesCache.Reset();
//        }

//        [Fact]
//        public async Task ShouldReturnCorrectStatus()
//        {
//            // Arrange
//            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();

//            var connectorId = Guid.NewGuid();
//            var networkId = Guid.NewGuid();
//            var connectorInfo = new ConnectorInfo(connectorId, networkId);
//            var taskStatus = new SingleTaskStatus("Task", "One of many tasks", 33.922);
//            var status1 = StatusLog.Create(connectorId, "Dummy message Error", StatusLogLevel.Error, DateTime.UtcNow);
//            var status2 = StatusLog.Create(connectorId, "Dummy message Info", StatusLogLevel.Info, DateTime.UtcNow);

//            await InitializeDatabase(connectorId, unitOfWorkFactory, status1, status2);

//            _tokenServiceMock
//                .Setup(x => x.HasValidAsync(connectorId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            _authTesterMock
//                .Setup(x => x.IsAuthorizedAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            _schedulerMock
//                .Setup(x => x.IsScheduledAsync(connectorInfo, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            _schedulerMock
//                .Setup(x => x.IsRunningAsync(connectorInfo, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            _tasksStatusesCache
//                .Setup(x => x.GetStatusAsync(connectorId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(taskStatus);

//            var statuService = new StatusService(unitOfWorkFactory, _tokenServiceMock.Object, _schedulerMock.Object, _tasksStatusesCache.Object, _authTesterMock.Object, NullLogger);

//            // Act
//            var result = await statuService.GetStatusAsync(connectorInfo);

//            // Assert
//            result.Authorized.Should().BeTrue();
//            result.Scheduled.Should().BeTrue();
//            result.Running.Should().BeTrue();
//            result.CurrentTask.Should().BeEquivalentTo(taskStatus);
//            result.Logs.Should().BeEquivalentTo(new[] { status1, status2 });
//        }

//        private static async Task InitializeDatabase(Guid connectorId, IUnitOfWorkFactory unitOfWorkFactory, params StatusLog[] logs)
//        {
//            var unitOfWork = unitOfWorkFactory.Create();
//            await InitializeNetwork(connectorId, unitOfWork);
//            await InitializeLogs(unitOfWork, logs);
//            await unitOfWork.CommitAsync();
//        }

//        private static async Task InitializeNetwork(Guid connectorId, IUnitOfWork unitOfWork)
//        {
//            var networkRepository = unitOfWork.GetConnectorRepository<TestableNetworkProperties>();
//            await networkRepository.AddAsync(Connector<TestableNetworkProperties>.Create(connectorId, new TestableNetworkProperties(), DateTime.UtcNow));
//        }

//        private static async Task InitializeLogs(IUnitOfWork unitOfWork, params StatusLog[] logs)
//        {
//            var statusLogRepository = unitOfWork.GetStatusLogRepository();
//            foreach (var log in logs)
//                await statusLogRepository.AddAsync(log);
//        }
//    }
//}

// TODO new implementation - new tests