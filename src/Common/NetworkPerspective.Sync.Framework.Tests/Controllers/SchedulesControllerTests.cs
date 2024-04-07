using System;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Framework.Dtos;

using Xunit;

namespace NetworkPerspective.Sync.Framework.Tests.Controllers
{
    public class SchedulesControllerTests
    {
        private readonly Mock<INetworkPerspectiveCore> _networkPerspectiveCoreMock = new Mock<INetworkPerspectiveCore>();
        private readonly Mock<ISyncScheduler> _schduleFacadeMock = new Mock<ISyncScheduler>();
        private readonly Mock<INetworkService> _networkServiceMock = new Mock<INetworkService>();
        private readonly Mock<ISyncHistoryService> _syncHistoryServiceMock = new Mock<ISyncHistoryService>();
        private readonly Mock<IStatusLoggerFactory> _statusLoggerFactoryMock = new Mock<IStatusLoggerFactory>();
        private readonly Mock<IStatusLogger> _statusLoggerMock = new Mock<IStatusLogger>();
        private readonly Mock<INetworkIdProvider> _networkIdProvider = new Mock<INetworkIdProvider>();

        public SchedulesControllerTests()
        {
            _networkPerspectiveCoreMock.Reset();
            _schduleFacadeMock.Reset();
            _networkServiceMock.Reset();
            _syncHistoryServiceMock.Reset();
            _statusLoggerFactoryMock.Reset();
            _statusLoggerMock.Reset();

            _statusLoggerFactoryMock
                .Setup(x => x.CreateForNetwork(It.IsAny<Guid>()))
                .Returns(_statusLoggerMock.Object);
        }

        public class Start : SchedulesControllerTests
        {
            [Fact]
            public async Task ShouldAddToSchedulerOnValidToken()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var accessToken = "access-token";

                _networkIdProvider
                    .Setup(x => x.Get())
                    .Returns(networkId);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => new NetworkCredential(string.Empty, x).Password == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new TokenValidationResponse(networkId, connectorId));

                var controller = Create(accessToken);

                // Act
                await controller.StartAsync(new SchedulerStartDto());

                // Assert
                _schduleFacadeMock.Verify(x => x.ScheduleAsync(networkId, It.IsAny<CancellationToken>()), Times.Once);
                _syncHistoryServiceMock.Verify(x => x.OverrideSyncStartAsync(networkId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task ShouldTriggerSyncOnValidToken()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var accessToken = "access-token";

                _networkIdProvider
                    .Setup(x => x.Get())
                    .Returns(networkId);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new TokenValidationResponse(networkId, connectorId));

                var controller = Create(accessToken);

                // Act
                await controller.StartAsync(new SchedulerStartDto());

                // Assert
                _schduleFacadeMock.Verify(x => x.TriggerNowAsync(networkId, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task ShouldNotAddToSchedulerOnNonExistingNetwork()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var accessToken = "access-token";

                _networkIdProvider
                    .Setup(x => x.Get())
                    .Returns(networkId);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new TokenValidationResponse(networkId, connectorId));

                _networkServiceMock
                    .Setup(x => x.ValidateExists(networkId, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new NetworkNotFoundException(networkId));

                var controller = Create(accessToken);
                Func<Task> func = () => controller.StartAsync(new SchedulerStartDto());

                // Act Assert
                await func.Should().ThrowExactlyAsync<NetworkNotFoundException>();
                _schduleFacadeMock.Verify(x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task ShouldOverrideSyncPeriodStart()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var accessToken = "access-token";
                var syncPeriodStart = DateTime.Now;

                _networkIdProvider
                    .Setup(x => x.Get())
                    .Returns(networkId);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new TokenValidationResponse(networkId, connectorId));

                var controller = Create(accessToken);

                // Act
                await controller.StartAsync(new SchedulerStartDto { OverrideSyncPeriodStart = syncPeriodStart });

                // Assert
                _syncHistoryServiceMock.Verify(x => x.OverrideSyncStartAsync(networkId, syncPeriodStart.ToUniversalTime(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        public class Stop : SchedulesControllerTests
        {
            [Fact]
            public async Task ShouldRemoveFromSchedulerOnValidToken()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var accessToken = "access-token";

                _networkIdProvider
                    .Setup(x => x.Get())
                    .Returns(networkId);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => new NetworkCredential(string.Empty, x).Password == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new TokenValidationResponse(networkId, connectorId));

                var controller = Create(accessToken);

                // Act
                await controller.StopAsync();

                // Assert
                _schduleFacadeMock.Verify(x => x.UnscheduleAsync(networkId, It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        protected SchedulesController Create(string accessToken)
        {
            var requestFeature = new HttpRequestFeature();
            requestFeature.Headers.Append(HeaderNames.Authorization, $"Bearer {accessToken}");

            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(requestFeature);

            var controller = new SchedulesController(_networkIdProvider.Object, _networkServiceMock.Object, _schduleFacadeMock.Object, _syncHistoryServiceMock.Object, _statusLoggerFactoryMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext(features);

            return controller;
        }
    }
}