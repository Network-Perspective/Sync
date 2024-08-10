using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Models;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Services
{
    public class MicrosoftAuthServiceTests
    {
        private readonly Mock<IAuthStateKeyFactory> _authStateKeyFactory = new();
        private readonly Mock<IVault> _secretRepositoryMock = new();
        private readonly Mock<IConnectorService> _networkServiceMock = new();
        private readonly Mock<IStatusLoggerFactory> _statusLoggerFactoryMock = new();
        private readonly Mock<IStatusLogger> _statusLoggerMock = new();
        private readonly ILogger<MicrosoftAuthService> _logger = NullLogger<MicrosoftAuthService>.Instance;

        public MicrosoftAuthServiceTests()
        {
            _authStateKeyFactory.Reset();
            _secretRepositoryMock.Reset();
            _networkServiceMock.Reset();
            _statusLoggerFactoryMock.Reset();
            _statusLoggerMock.Reset();

            _statusLoggerFactoryMock
                .Setup(x => x.CreateForConnector(It.IsAny<Guid>()))
                .Returns(_statusLoggerMock.Object);
        }

        public class StartAuthProcessAsync : MicrosoftAuthServiceTests
        {
            [Fact]
            public async Task ShouldBuildCorrectAuthUri()
            {
                // Arrange
                const string redirectUrl = "https://networkperspective.io:5001/callback";
                var connectorId = Guid.NewGuid();
                var clientId = Guid.NewGuid().ToString();
                var state = Guid.NewGuid().ToString();

                _authStateKeyFactory
                    .Setup(x => x.Create())
                    .Returns(state);

                _secretRepositoryMock
                    .Setup(x => x.GetSecretAsync(MicrosoftKeys.MicrosoftClientBasicIdKey, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(clientId.ToSecureString());

                var connectorProperties = new MicrosoftNetworkProperties(false, false, false, false, null);
                var connector = Connector<MicrosoftNetworkProperties>.Create(connectorId, connectorProperties, DateTime.UtcNow);
                _networkServiceMock
                    .Setup(x => x.GetAsync<MicrosoftNetworkProperties>(connectorId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(connector);

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
                var service = new MicrosoftAuthService(
                    _authStateKeyFactory.Object,
                    _secretRepositoryMock.Object,
                    cache,
                    _statusLoggerFactoryMock.Object,
                    _networkServiceMock.Object,
                    _logger);

                // Act
                var result = await service.StartAuthProcessAsync(new AuthProcess(connectorId, new Uri(redirectUrl)));

                // Assert
                var resultUri = new Uri(result.MicrosoftAuthUri);
                resultUri.Scheme.Should().Be("https");
                resultUri.Host.Should().Be("login.microsoftonline.com");
                resultUri.LocalPath.Should().Be("/common/adminconsent");
                HttpUtility.ParseQueryString(resultUri.Query).Get("client_id").Should().Be(clientId);
                HttpUtility.ParseQueryString(resultUri.Query).Get("state").Should().Be(state);
                HttpUtility.ParseQueryString(resultUri.Query).Get("redirect_uri").Should().Be(redirectUrl);
            }

            [Fact]
            public void ShouldPutStateToCache()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var callbackUri = new Uri("https://localhost:5001/callback");
                var authProcess = new AuthProcess(connectorId, callbackUri);

                var state = Guid.NewGuid().ToString();

                _authStateKeyFactory
                    .Setup(x => x.Create())
                    .Returns(state);

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
                var service = new MicrosoftAuthService(
                    _authStateKeyFactory.Object,
                    _secretRepositoryMock.Object,
                    cache,
                    _statusLoggerFactoryMock.Object,
                    _networkServiceMock.Object,
                    _logger);

                // Act
                var result = service.StartAuthProcessAsync(authProcess);

                // Assert
                cache.Get(state).Should().Be(authProcess);
            }
        }

        public class HandleCallback : MicrosoftAuthServiceTests
        {
            [Fact]
            public async Task ShouldThrowAuthExceptionOnNonExistingState()
            {
                // Arrange
                var state = "non-exiting-state";

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

                var service = new MicrosoftAuthService(
                    _authStateKeyFactory.Object,
                    _secretRepositoryMock.Object,
                    cache,
                    _statusLoggerFactoryMock.Object,
                    _networkServiceMock.Object,
                    _logger);

                // Act
                Func<Task> func = () => service.HandleCallbackAsync(Guid.NewGuid(), state);

                // Assert
                await func.Should().ThrowAsync<OAuthException>();
            }
        }
    }
}