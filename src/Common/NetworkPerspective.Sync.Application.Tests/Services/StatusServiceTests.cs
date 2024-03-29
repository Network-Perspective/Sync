﻿using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class StatusServiceTests
    {
        private static readonly ILogger<StatusService> NullLogger = NullLogger<StatusService>.Instance;
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<ISyncScheduler> _schedulerMock = new();
        private readonly Mock<ITasksStatusesCache> _tasksStatusesCache = new();
        private readonly Mock<IAuthTester> _authTesterMock = new();

        public StatusServiceTests()
        {
            _tokenServiceMock.Reset();
            _schedulerMock.Reset();
            _tasksStatusesCache.Reset();
        }

        [Fact]
        public async Task ShouldReturnCorrectStatus()
        {
            // Arrange
            var unitOfWorkFactory = new InMemoryUnitOfWorkFactory();

            var networkId = Guid.NewGuid();
            var taskStatus = new SingleTaskStatus("Task", "One of many tasks", 33.922);
            var status1 = StatusLog.Create(networkId, "Dummy message Error", StatusLogLevel.Error, DateTime.UtcNow);
            var status2 = StatusLog.Create(networkId, "Dummy message Info", StatusLogLevel.Info, DateTime.UtcNow);

            await InitializeDatabase(networkId, unitOfWorkFactory, status1, status2);

            _tokenServiceMock
                .Setup(x => x.HasValidAsync(networkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _authTesterMock
                .Setup(x => x.IsAuthorizedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _schedulerMock
                .Setup(x => x.IsScheduledAsync(networkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _schedulerMock
                .Setup(x => x.IsRunningAsync(networkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _tasksStatusesCache
                .Setup(x => x.GetStatusAsync(networkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskStatus);

            var statuService = new StatusService(unitOfWorkFactory, _tokenServiceMock.Object, _schedulerMock.Object, _tasksStatusesCache.Object, _authTesterMock.Object, NullLogger);

            // Act
            var result = await statuService.GetStatusAsync(networkId);

            // Assert
            result.Authorized.Should().BeTrue();
            result.Scheduled.Should().BeTrue();
            result.Running.Should().BeTrue();
            result.CurrentTask.Should().BeEquivalentTo(taskStatus);
            result.Logs.Should().BeEquivalentTo(new[] { status1, status2 });
        }

        private static async Task InitializeDatabase(Guid networkId, IUnitOfWorkFactory unitOfWorkFactory, params StatusLog[] logs)
        {
            var unitOfWork = unitOfWorkFactory.Create();
            await InitializeNetwork(networkId, unitOfWork);
            await InitializeLogs(unitOfWork, logs);
            await unitOfWork.CommitAsync();
        }

        private static async Task InitializeNetwork(Guid networkId, IUnitOfWork unitOfWork)
        {
            var networkRepository = unitOfWork.GetNetworkRepository<TestableNetworkProperties>();
            await networkRepository.AddAsync(Network<TestableNetworkProperties>.Create(networkId, new TestableNetworkProperties(), DateTime.UtcNow));
        }

        private static async Task InitializeLogs(IUnitOfWork unitOfWork, params StatusLog[] logs)
        {
            var statusLogRepository = unitOfWork.GetStatusLogRepository();
            foreach (var log in logs)
                await statusLogRepository.AddAsync(log);
        }
    }
}