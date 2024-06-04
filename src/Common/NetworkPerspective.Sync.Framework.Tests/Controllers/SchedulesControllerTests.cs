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

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Framework.Dtos;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Framework.Tests.Controllers
{
    public class SchedulesControllerTests
    {
        private readonly Mock<INetworkPerspectiveCore> _networkPerspectiveCoreMock = new Mock<INetworkPerspectiveCore>();
        private readonly Mock<ISyncScheduler> _schduleFacadeMock = new Mock<ISyncScheduler>();
        private readonly Mock<IConnectorService> _networkServiceMock = new Mock<IConnectorService>();
        private readonly Mock<ISyncHistoryService> _syncHistoryServiceMock = new Mock<ISyncHistoryService>();
        private readonly Mock<IStatusLoggerFactory> _statusLoggerFactoryMock = new Mock<IStatusLoggerFactory>();
        private readonly Mock<IStatusLogger> _statusLoggerMock = new Mock<IStatusLogger>();
        private readonly Mock<IConnectorInfoProvider> _connectorInfoProvider = new Mock<IConnectorInfoProvider>();

        public SchedulesControllerTests()
        {
            _networkPerspectiveCoreMock.Reset();
            _schduleFacadeMock.Reset();
            _networkServiceMock.Reset();
            _syncHistoryServiceMock.Reset();
            _statusLoggerFactoryMock.Reset();
            _statusLoggerMock.Reset();

            _statusLoggerFactoryMock
                .Setup(x => x.CreateForConnector(It.IsAny<Guid>()))
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
                var connectionInfo = new ConnectorInfo(connectorId, networkId);
                var accessToken = "access-token";

                _connectorInfoProvider
                    .Setup(x => x.Get())
                    .Returns(connectionInfo);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => new NetworkCredential(string.Empty, x).Password == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connectionInfo);

                var controller = Create(accessToken);

                // Act
                await controller.StartAsync(new SchedulerStartDto());

                // Assert
                _schduleFacadeMock.Verify(x => x.ScheduleAsync(connectorId, It.IsAny<CancellationToken>()), Times.Once);
                _syncHistoryServiceMock.Verify(x => x.OverrideSyncStartAsync(connectorId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task ShouldTriggerSyncOnValidToken()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var connectionInfo = new ConnectorInfo(connectorId, networkId);
                var accessToken = "access-token";

                _connectorInfoProvider
                    .Setup(x => x.Get())
                    .Returns(connectionInfo);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connectionInfo);

                var controller = Create(accessToken);

                // Act
                await controller.StartAsync(new SchedulerStartDto());

                // Assert
                _schduleFacadeMock.Verify(x => x.TriggerNowAsync(connectorId, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task ShouldNotAddToSchedulerOnNonExistingNetwork()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var connectionInfo = new ConnectorInfo(connectorId, networkId);
                var accessToken = "access-token";

                _connectorInfoProvider
                    .Setup(x => x.Get())
                    .Returns(connectionInfo);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connectionInfo);

                _networkServiceMock
                    .Setup(x => x.ValidateExists(connectorId, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ConnectorNotFoundException(networkId));

                var controller = Create(accessToken);
                Func<Task> func = () => controller.StartAsync(new SchedulerStartDto());

                // Act Assert
                await func.Should().ThrowExactlyAsync<ConnectorNotFoundException>();
                _schduleFacadeMock.Verify(x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task ShouldOverrideSyncPeriodStart()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var connectionInfo = new ConnectorInfo(connectorId, networkId);
                var accessToken = "access-token";
                var syncPeriodStart = DateTime.Now;

                _connectorInfoProvider
                    .Setup(x => x.Get())
                    .Returns(connectionInfo);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connectionInfo);

                var controller = Create(accessToken);

                // Act
                await controller.StartAsync(new SchedulerStartDto { OverrideSyncPeriodStart = syncPeriodStart });

                // Assert
                _syncHistoryServiceMock.Verify(x => x.OverrideSyncStartAsync(connectorId, syncPeriodStart.ToUniversalTime(), It.IsAny<CancellationToken>()), Times.Once);
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
                var connectionInfo = new ConnectorInfo(connectorId, networkId);
                var accessToken = "access-token";

                _connectorInfoProvider
                    .Setup(x => x.Get())
                    .Returns(connectionInfo);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => new NetworkCredential(string.Empty, x).Password == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connectionInfo);

                var controller = Create(accessToken);

                // Act
                await controller.StopAsync();

                // Assert
                _schduleFacadeMock.Verify(x => x.UnscheduleAsync(connectorId, It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        protected SchedulesController Create(string accessToken)
        {
            var requestFeature = new HttpRequestFeature();
            requestFeature.Headers.Append(HeaderNames.Authorization, $"Bearer {accessToken}");

            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(requestFeature);

            var controller = new SchedulesController(_connectorInfoProvider.Object, _networkServiceMock.Object, _schduleFacadeMock.Object, _syncHistoryServiceMock.Object, _statusLoggerFactoryMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext(features);

            return controller;
        }
    }
}