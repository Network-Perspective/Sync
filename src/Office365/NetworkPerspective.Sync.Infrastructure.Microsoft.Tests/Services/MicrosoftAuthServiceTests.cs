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

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Models;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Services
{
    public class MicrosoftAuthServiceTests
    {
        private readonly Mock<IAuthStateKeyFactory> _authStateKeyFactory = new Mock<IAuthStateKeyFactory>();
        private readonly Mock<ISecretRepositoryFactory> _secretRepositoryFactoryMock = new Mock<ISecretRepositoryFactory>();
        private readonly Mock<ISecretRepository> _secretRepositoryMock = new Mock<ISecretRepository>();
        private readonly Mock<INetworkService> _networkServiceMock = new Mock<INetworkService>();
        private readonly Mock<IStatusLoggerFactory> _statusLoggerFactoryMock = new Mock<IStatusLoggerFactory>();
        private readonly Mock<IStatusLogger> _statusLoggerMock = new Mock<IStatusLogger>();
        private readonly ILogger<MicrosoftAuthService> _logger = NullLogger<MicrosoftAuthService>.Instance;

        public MicrosoftAuthServiceTests()
        {
            _authStateKeyFactory.Reset();
            _secretRepositoryFactoryMock.Reset();
            _secretRepositoryMock.Reset();
            _networkServiceMock.Reset();
            _statusLoggerFactoryMock.Reset();
            _statusLoggerMock.Reset();

            _secretRepositoryFactoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_secretRepositoryMock.Object);

            _statusLoggerFactoryMock
                .Setup(x => x.CreateForNetwork(It.IsAny<Guid>()))
                .Returns(_statusLoggerMock.Object);
        }

        public class StartAuthProcessAsync : MicrosoftAuthServiceTests
        {
            [Fact]
            public async Task ShouldBuildCorrectAuthUri()
            {
                // Arrange
                const string redirectUrl = "https://networkperspective.io:5001/callback";
                var networkId = Guid.NewGuid();
                var clientId = Guid.NewGuid().ToString();
                var state = Guid.NewGuid().ToString();

                _authStateKeyFactory
                    .Setup(x => x.Create())
                    .Returns(state);

                _secretRepositoryMock
                    .Setup(x => x.GetSecretAsync(MicrosoftKeys.MicrosoftClientBasicIdKey, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(clientId.ToSecureString());

                var networkProperties = new MicrosoftNetworkProperties(false, false, false, false, null);
                var network = Network<MicrosoftNetworkProperties>.Create(networkId, networkProperties, DateTime.UtcNow);
                _networkServiceMock
                    .Setup(x => x.GetAsync<MicrosoftNetworkProperties>(networkId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(network);

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
                var service = new MicrosoftAuthService(
                    _authStateKeyFactory.Object,
                    _secretRepositoryFactoryMock.Object,
                    cache,
                    _statusLoggerFactoryMock.Object,
                    _networkServiceMock.Object,
                    _logger);

                // Act
                var result = await service.StartAuthProcessAsync(new AuthProcess(networkId, new Uri(redirectUrl)));

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
                var networkId = Guid.NewGuid();
                var callbackUri = new Uri("https://localhost:5001/callback");
                var authProcess = new AuthProcess(networkId, callbackUri);

                var state = Guid.NewGuid().ToString();

                _authStateKeyFactory
                    .Setup(x => x.Create())
                    .Returns(state);

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
                var service = new MicrosoftAuthService(
                    _authStateKeyFactory.Object,
                    _secretRepositoryFactoryMock.Object,
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
                    _secretRepositoryFactoryMock.Object,
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