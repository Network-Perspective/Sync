using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.SlackAuth;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.SlackAuth;

public class SlackAuthServiceTests
{
    private const string ClientId = "1234";
    private static readonly string[] Scopes = ["scope1", "scope2"];
    private static readonly string[] UserScopes = ["userScope1", "userScope2"];
    private static readonly string[] AdminUserScopes = ["adminUserScope1", "adminUserScope2"];

    private readonly Mock<IVault> _vaultMock = new();
    private readonly Mock<IAuthStateKeyFactory> _stateFactoryMock = new();
    private readonly Mock<IWorkerRouter> _workerRouter = new();
    private readonly Mock<ISlackClientUnauthorizedFacade> _slackClientUnauthorizedFacadeMock = new();
    private readonly ILogger<SlackAuthService> _logger = NullLogger<SlackAuthService>.Instance;

    public SlackAuthServiceTests()
    {
        _vaultMock.Reset();
        _stateFactoryMock.Reset();
        _workerRouter.Reset();
        _slackClientUnauthorizedFacadeMock.Reset();
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

            _vaultMock
                .Setup(x => x.GetSecretAsync(SlackAuthService.SlackClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var service = new SlackAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _slackClientUnauthorizedFacadeMock.Object,
                _logger);

            // Act
            var result = await service.StartAuthProcessAsync(new SlackAuthProcess(Guid.NewGuid(), "worker-name", new Uri(redirectUrl), false));

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

            _vaultMock
                .Setup(x => x.GetSecretAsync(SlackAuthService.SlackClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var service = new SlackAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _slackClientUnauthorizedFacadeMock.Object,
                _logger);

            // Act
            var result = await service.StartAuthProcessAsync(new SlackAuthProcess(Guid.NewGuid(), "worker-name", new Uri(redirectUrl), true));
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
            var authProcess = new SlackAuthProcess(networkId, "worker-name", callbackUri, false);

            var stateKey = Guid.NewGuid().ToString();
            var config = CreateDefaultOptions();

            _stateFactoryMock
                .Setup(x => x.Create())
                .Returns(stateKey);

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new SlackAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _slackClientUnauthorizedFacadeMock.Object,
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
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _slackClientUnauthorizedFacadeMock.Object,
                _logger);

            // Act
            Func<Task> func = () => service.HandleAuthorizationCodeCallbackAsync("foo", state);

            // Assert
            await func.Should().ThrowAsync<OAuthException>();
        }

        [Fact]
        public async Task ShouldSendTokensToWorker()
        {
            // Arrange
            var state = "state-key";
            var workerName = "worker-name";

            var config = CreateDefaultOptions();

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var authState = new SlackAuthProcess(Guid.NewGuid(), workerName, new Uri("https://networkperspective.io/"), false);
            cache.Set(state, authState);

            var accessResponse = new OAuthAccessResponse
            {
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString()
            };
            _slackClientUnauthorizedFacadeMock
                .Setup(x => x.AccessAsync(It.IsAny<OAuthAccessRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessResponse);

            var service = new SlackAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _slackClientUnauthorizedFacadeMock.Object,
                _logger);

            // Act
            await service.HandleAuthorizationCodeCallbackAsync("foo", state);

            // Assert
            _workerRouter.Verify(x => x.SetSecretsAsync(workerName, It.IsAny<IDictionary<string, SecureString>>()), Times.Once);
        }
    }

    private static IOptions<SlackAuthConfig> CreateDefaultOptions()
        => Options.Create(new SlackAuthConfig
        {
            Scopes = Scopes,
            UserScopes = UserScopes,
            AdminUserScopes = AdminUserScopes,
        });
}