using System;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<ISecretRepositoryFactory> _secretRepositoryFactoryMock = new();
        private readonly Mock<ISecretRepository> _secretRepositoryMock = new();
        private readonly Mock<IConnectorService> _connectorServiceMock = new();
        private readonly Mock<INetworkPerspectiveCore> _networkPerspectiveCoreMock = new();
        private readonly ILogger<TokenService> _logger = NullLogger<TokenService>.Instance;

        public TokenServiceTests()
        {
            _secretRepositoryFactoryMock.Reset();
            _secretRepositoryMock.Reset();
            _connectorServiceMock.Reset();
            _networkPerspectiveCoreMock.Reset();

            _secretRepositoryFactoryMock
                .Setup(x => x.Create(It.IsAny<Uri>()))
                .Returns(_secretRepositoryMock.Object);
        }

        public class SaveAccessToken : TokenServiceTests
        {
            [Fact]
            public async Task ShouldSaveAccessToken()
            {
                // Arrange
                const string dataSourceName = "foo";
                const string accessToken = "bar";
                var connectorId = Guid.NewGuid();

                var connector = Connector<ConnectorProperties>.Create(connectorId, ConnectorProperties.Create<ConnectorProperties>([]), DateTime.UtcNow);

                _connectorServiceMock
                    .Setup(x => x.GetAsync<ConnectorProperties>(connectorId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connector);

                var service = new TokenService(_secretRepositoryFactoryMock.Object, _connectorServiceMock.Object, _networkPerspectiveCoreMock.Object, CreateOptions(dataSourceName), _logger);

                // Act
                await service.AddOrReplace(accessToken.ToSecureString(), connectorId);

                // Assert
                var expectedKey = $"np-token-{dataSourceName}-{connectorId}";
                _secretRepositoryMock
                    .Verify(x => x.SetSecretAsync(
                        expectedKey,
                        It.Is<SecureString>(x => x.ToSystemString() == accessToken),
                        It.IsAny<CancellationToken>()));
            }
        }

        public class GetAccessToken : TokenServiceTests
        {
            [Fact]
            public async Task ShouldReturnAccessToken()
            {
                // Arrange
                const string dataSourceName = "foo";
                const string accessToken = "bar";
                var connectorId = Guid.NewGuid();
                var key = $"np-token-{dataSourceName}-{connectorId}";

                var connector = Connector<ConnectorProperties>.Create(connectorId, ConnectorProperties.Create<ConnectorProperties>([]), DateTime.UtcNow);

                _connectorServiceMock
                    .Setup(x => x.GetAsync<ConnectorProperties>(connectorId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connector);

                _secretRepositoryMock
                    .Setup(x => x.GetSecretAsync(key, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(accessToken.ToSecureString());

                var service = new TokenService(_secretRepositoryFactoryMock.Object, _connectorServiceMock.Object, _networkPerspectiveCoreMock.Object, CreateOptions(dataSourceName), _logger);

                // Act
                var result = await service.GetAsync(connectorId);

                // Assert
                result.ToSystemString().Should().Be(accessToken);
            }
        }

        public class HasValidAccessToken : TokenServiceTests
        {
            [Fact]
            public async Task ShouldReturnTrueOnHavingValidToken()
            {
                // Arrange
                const string dataSourceName = "foo";
                const string accessToken = "bar";
                var connectorId = Guid.NewGuid();
                var key = $"np-token-{dataSourceName}-{connectorId}";

                var connector = Connector<ConnectorProperties>.Create(connectorId, ConnectorProperties.Create<ConnectorProperties>([]), DateTime.UtcNow);

                _connectorServiceMock
                    .Setup(x => x.GetAsync<ConnectorProperties>(connectorId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connector);

                _secretRepositoryMock
                    .Setup(x => x.GetSecretAsync(key, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(accessToken.ToSecureString());

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ConnectorInfo(connectorId, Guid.NewGuid()));

                var service = new TokenService(_secretRepositoryFactoryMock.Object, _connectorServiceMock.Object, _networkPerspectiveCoreMock.Object, CreateOptions(dataSourceName), _logger);

                // Act
                var result = await service.HasValidAsync(connectorId);

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public async Task ShouldReturnFalseOnInvalidToken()
            {
                // Arrange
                const string dataSourceName = "foo";
                const string accessToken = "bar";
                var connectorId = Guid.NewGuid();
                var key = $"np-token-{dataSourceName}-{connectorId}";

                var connector = Connector<ConnectorProperties>.Create(connectorId, ConnectorProperties.Create<ConnectorProperties>([]), DateTime.UtcNow);

                _connectorServiceMock
                    .Setup(x => x.GetAsync<ConnectorProperties>(connectorId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connector);

                _secretRepositoryMock
                    .Setup(x => x.GetSecretAsync(key, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(accessToken.ToSecureString());

                _networkPerspectiveCoreMock
                    .Setup(x => x.ValidateTokenAsync(It.Is<SecureString>(x => x.ToSystemString() == accessToken), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidTokenException("https://foo/bar"));

                var service = new TokenService(_secretRepositoryFactoryMock.Object, _connectorServiceMock.Object, _networkPerspectiveCoreMock.Object, CreateOptions(dataSourceName), _logger);

                // Act
                var result = await service.HasValidAsync(connectorId);

                // Assert
                result.Should().BeFalse();
            }
        }

        public class EnsureRemoved : TokenServiceTests
        {
            [Fact]
            public async Task ShouldNotThrowOnNonExistingSecret()
            {
                // Arrange
                const string dataSourceName = "foo";

                var connectorId = Guid.NewGuid();
                var key = $"np-token-{dataSourceName}-{connectorId}";

                var connector = Connector<ConnectorProperties>.Create(connectorId, ConnectorProperties.Create<ConnectorProperties>([]), DateTime.UtcNow);

                _connectorServiceMock
                    .Setup(x => x.GetAsync<ConnectorProperties>(connectorId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connector);

                _secretRepositoryMock
                    .Setup(x => x.GetSecretAsync(key, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new SecretStorageException("message", new Exception()));

                var service = new TokenService(_secretRepositoryFactoryMock.Object, _connectorServiceMock.Object, _networkPerspectiveCoreMock.Object, CreateOptions(dataSourceName), _logger);
                Func<Task> func = () => service.EnsureRemovedAsync(connectorId);

                // Act Assert
                await func.Should().NotThrowAsync();
            }
        }

        private static IOptions<MiscConfig> CreateOptions(string dataSourceName)
            => Options.Create(new MiscConfig { DataSourceName = dataSourceName });
    }
}