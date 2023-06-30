using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;
using NetworkPerspective.Sync.Infrastructure.Slack.Models;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Services
{
    public class SlackAuthServiceTests
    {
        private const string ClientId = "1234";
        private static readonly string[] Scopes = new[] { "scope1", "scope2" };
        private static readonly string[] UserScopes = new[] { "userScope1", "userScope2" };
        private static readonly string[] AdminUserScopes = new[] { "adminUserScope1", "adminUserScope2" };

        private readonly Mock<IAuthStateKeyFactory> _stateFactoryMock = new();
        private readonly Mock<ISecretRepositoryFactory> _secretRepositoryFactoryMock = new();
        private readonly Mock<ISecretRepository> _secretRepositoryMock = new();
        private readonly Mock<ISlackClientUnauthorizedFacade> _slackClientUnauthorizedFacadeMock = new();
        private readonly Mock<IStatusLoggerFactory> _statusLoggerFactoryMock = new();
        private readonly Mock<IStatusLogger> _statusLoggerMock = new();
        private readonly ILogger<SlackAuthService> _logger = NullLogger<SlackAuthService>.Instance;

        public SlackAuthServiceTests()
        {
            _stateFactoryMock.Reset();
            _secretRepositoryFactoryMock.Reset();
            _secretRepositoryMock.Reset();
            _slackClientUnauthorizedFacadeMock.Reset();
            _statusLoggerFactoryMock.Reset();
            _statusLoggerMock.Reset();

            _secretRepositoryFactoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_secretRepositoryMock.Object);

            _statusLoggerFactoryMock
                .Setup(x => x.CreateForNetwork(It.IsAny<Guid>()))
                .Returns(_statusLoggerMock.Object);
        }

        public class StartAuthProcess : SlackAuthServiceTests
        {
            [Fact]
            public async Task ShouldBuildCorrectAuthUriWithoutAdminPrivileges()
            {
                // Arrange
                var state = Guid.NewGuid().ToString();
                var config = CreateDefaultOptions();
                const string redirectUrl = "https://networkperspective.io:5001/callback";

                _stateFactoryMock
                    .Setup(x => x.Create())
                    .Returns(state);

                _secretRepositoryMock
                    .Setup(x => x.GetSecretAsync(SlackKeys.SlackClientIdKey, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(ClientId.ToSecureString());

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

                var service = new SlackAuthService(
                    _stateFactoryMock.Object,
                    config,
                    _secretRepositoryFactoryMock.Object,
                    _slackClientUnauthorizedFacadeMock.Object,
                    cache,
                    _statusLoggerFactoryMock.Object,
                    _logger);

                // Act
                var result = await service.StartAuthProcessAsync(new AuthProcess(Guid.NewGuid(), new Uri(redirectUrl), false));

                // Assert
                result.SlackAuthUri.Should().Contain("oauth/v2/authorize");
                result.SlackAuthUri.Should().Contain($"client_id={ClientId}");
                result.SlackAuthUri.Should().Contain($"scope={string.Join(',', Scopes)}");
                result.SlackAuthUri.Should().Contain($"user_scope={string.Join(',', UserScopes)}");
                result.SlackAuthUri.Should().Contain($"redirect_uri={redirectUrl}");
            }

            [Fact]
            public async Task ShouldBuildCorrectAuthUriWithAdminPrivileges()
            {
                // Arrange
                var state = Guid.NewGuid().ToString();
                var config = CreateDefaultOptions();
                const string redirectUrl = "https://networkperspective.io:5001/callback";

                _stateFactoryMock
                    .Setup(x => x.Create())
                    .Returns(state);

                _secretRepositoryMock
                    .Setup(x => x.GetSecretAsync(SlackKeys.SlackClientIdKey, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(ClientId.ToSecureString());

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

                var service = new SlackAuthService(
                    _stateFactoryMock.Object,
                    config,
                    _secretRepositoryFactoryMock.Object,
                    _slackClientUnauthorizedFacadeMock.Object,
                    cache,
                    _statusLoggerFactoryMock.Object,
                    _logger);

                // Act
                var result = await service.StartAuthProcessAsync(new AuthProcess(Guid.NewGuid(), new Uri(redirectUrl), true));

                // Assert
                result.SlackAuthUri.Should().Contain("oauth/v2/authorize");
                result.SlackAuthUri.Should().Contain($"client_id={ClientId}");
                result.SlackAuthUri.Should().Contain($"scope={string.Join(',', Scopes)}");
                result.SlackAuthUri.Should().Contain($"user_scope={string.Join(',', UserScopes.Union(AdminUserScopes))}");
                result.SlackAuthUri.Should().Contain($"redirect_uri={redirectUrl}");
            }

            [Fact]
            public void ShouldPutStateToCache()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var callbackUri = new Uri("https://localhost:5001/callback");
                var authProcess = new AuthProcess(networkId, callbackUri, false);

                var stateKey = Guid.NewGuid().ToString();
                var config = CreateDefaultOptions();

                _stateFactoryMock
                    .Setup(x => x.Create())
                    .Returns(stateKey);

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
                var service = new SlackAuthService(
                    _stateFactoryMock.Object,
                    config,
                    _secretRepositoryFactoryMock.Object,
                    _slackClientUnauthorizedFacadeMock.Object,
                    cache,
                    _statusLoggerFactoryMock.Object,
                    _logger);

                // Act
                var result = service.StartAuthProcessAsync(authProcess);

                // Assert
                cache.Get(stateKey).Should().Be(authProcess);
            }
        }

        public class HandleAuthorizationCodeCallback : SlackAuthServiceTests
        {
            [Fact]
            public async Task ShouldThrowAuthExceptionOnNonExistingState()
            {
                // Arrange
                var state = "non-exiting-state";

                var config = CreateDefaultOptions();

                var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

                var service = new SlackAuthService(
                    _stateFactoryMock.Object,
                    config,
                    _secretRepositoryFactoryMock.Object,
                    _slackClientUnauthorizedFacadeMock.Object,
                    cache,
                    _statusLoggerFactoryMock.Object,
                    _logger);

                // Act
                Func<Task> func = () => service.HandleAuthorizationCodeCallbackAsync("foo", state);

                // Assert
                await func.Should().ThrowAsync<OAuthException>();
            }
        }

        private static IOptions<AuthConfig> CreateDefaultOptions()
            => Options.Create(new AuthConfig
            {
                Scopes = Scopes,
                UserScopes = UserScopes,
                AdminUserScopes = AdminUserScopes,
            });
    }
}