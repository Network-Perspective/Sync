using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Framework.Tests.Controllers
{
    public class StatusControllerTests
    {
        private readonly Mock<INetworkPerspectiveCore> _networkPerspectiveCoreMock = new Mock<INetworkPerspectiveCore>();
        private readonly Mock<IStatusService> _statusServiceMock = new Mock<IStatusService>();
        private readonly Mock<INetworkService> _networkServiceMock = new Mock<INetworkService>();
        private readonly Mock<INetworkIdProvider> _networkIdProvider = new Mock<INetworkIdProvider>();

        public StatusControllerTests()
        {
            _networkPerspectiveCoreMock.Reset();
            _statusServiceMock.Reset();
            _networkServiceMock.Reset();
        }

        public class GetStatus : StatusControllerTests
        {
            [Fact]
            public async Task ShouldReturnStatus()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var connectorId = Guid.NewGuid();
                var accessToken = "access-token";

                var status = new Status
                {
                    Authorized = false,
                    Running = false,
                    Scheduled = false,
                    Logs = Array.Empty<StatusLog>()
                };

                _networkIdProvider
                    .Setup(x => x.Get())
                    .Returns(networkId);

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new TokenValidationResponse(networkId, connectorId));

                _statusServiceMock
                    .Setup(x => x.GetStatusAsync(networkId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(status);

                var controller = Create(accessToken);

                // Act
                var result = await controller.GetStatus();

                // Assert
                result.Should().BeEquivalentTo(status);
            }

            [Fact]
            public async Task ShoulThrowNetworkNotFoundOnNonExistingNetwork()
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
                Func<Task> func = () => controller.GetStatus();

                // Act Assert
                await func.Should().ThrowExactlyAsync<NetworkNotFoundException>();
                _statusServiceMock.Verify(x => x.GetStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        protected StatusController Create(string accessToken)
        {
            var requestFeature = new HttpRequestFeature();
            requestFeature.Headers.Append(HeaderNames.Authorization, $"Bearer {accessToken}");

            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(requestFeature);

            var controller = new StatusController(_networkServiceMock.Object, _statusServiceMock.Object, _networkIdProvider.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext(features);

            return controller;
        }
    }
}