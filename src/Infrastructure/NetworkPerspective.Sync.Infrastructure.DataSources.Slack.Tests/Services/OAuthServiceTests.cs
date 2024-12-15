using System;
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
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Tests.Services;

public class OAuthServiceTests
{
    private const string ClientId = "1234";
    private static readonly string[] Scopes = ["scope1", "scope2"];
    private static readonly string[] UserScopes = ["userScope1", "userScope2"];
    private static readonly string[] AdminUserScopes = ["adminUserScope1", "adminUserScope2"];

    private readonly Mock<IVault> _vaultMock = new();
    private readonly Mock<IAuthStateKeyFactory> _stateFactoryMock = new();
    private readonly Mock<ISlackClientUnauthorizedFacade> _slackClientUnauthorizedFacadeMock = new();
    private readonly ILogger<OAuthService> _logger = NullLogger<OAuthService>.Instance;

    public OAuthServiceTests()
    {
        _vaultMock.Reset();
        _stateFactoryMock.Reset();
        _slackClientUnauthorizedFacadeMock.Reset();
    }

    public class StartAuthProcess : OAuthServiceTests
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
                .Setup(x => x.GetSecretAsync(OAuthService.SlackClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var service = new OAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                config,
                _slackClientUnauthorizedFacadeMock.Object,
                _logger);

            var connectorProperties = new Dictionary<string, string>
            {
                { "UsesAdminPrivileges", "false" }
            };
            var connectorInfo = new ConnectorContext(Guid.NewGuid(), "Slack", connectorProperties);
            // Act
            var result = await service.InitializeOAuthAsync(new OAuthContext(connectorInfo, redirectUrl));

            // Assert
            result.AuthUri.Should().Contain("oauth/v2/authorize");
            result.AuthUri.Should().Contain($"client_id={ClientId}");
            result.AuthUri.Should().Contain($"scope={string.Join(',', Scopes)}");
            result.AuthUri.Should().Contain($"user_scope={string.Join(',', UserScopes)}");
            result.AuthUri.Should().Contain($"redirect_uri={redirectUrl}");
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
                .Setup(x => x.GetSecretAsync(OAuthService.SlackClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var service = new OAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                config,
                _slackClientUnauthorizedFacadeMock.Object,
                _logger);

            var connectorProperties = new Dictionary<string, string>
            {
                { "UsesAdminPrivileges", "true" }
            };
            var connectorInfo = new ConnectorContext(Guid.NewGuid(), "Slack", connectorProperties);

            // Act
            var result = await service.InitializeOAuthAsync(new OAuthContext(connectorInfo, redirectUrl));

            // Assert
            result.AuthUri.Should().Contain("oauth/v2/authorize");
            result.AuthUri.Should().Contain($"client_id={ClientId}");
            result.AuthUri.Should().Contain($"scope={string.Join(',', Scopes)}");
            result.AuthUri.Should().Contain($"user_scope={string.Join(',', UserScopes.Union(AdminUserScopes))}");
            result.AuthUri.Should().Contain($"redirect_uri={redirectUrl}");
        }

        [Fact]
        public void ShouldPutStateToCache()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var callbackUri = "https://localhost:5001/callback";
            var connectorInfo = new ConnectorContext(connectorId, "Slack", new Dictionary<string, string>());
            var context = new OAuthContext(connectorInfo, callbackUri);

            var stateKey = Guid.NewGuid().ToString();
            var config = CreateDefaultOptions();

            _stateFactoryMock
                .Setup(x => x.Create())
                .Returns(stateKey);

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new OAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                config,
                _slackClientUnauthorizedFacadeMock.Object,
                _logger);

            // Act
            var result = service.InitializeOAuthAsync(context);

            // Assert
            cache.Get(stateKey).Should().Be(context);
        }
    }

    public class HandleAuthorizationCodeCallback : OAuthServiceTests
    {
        [Fact]
        public async Task ShouldSetKeysInVault()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var callbackUri = "https://localhost:5001/callback";

            var config = CreateDefaultOptions();

            var connectorInfo = new ConnectorContext(connectorId, "Slack", new Dictionary<string, string>());
            var context = new OAuthContext(connectorInfo, callbackUri);


            var accessResponse = new OAuthAccessResponse
            {
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString()
            };
            _slackClientUnauthorizedFacadeMock
                .Setup(x => x.AccessAsync(It.IsAny<OAuthAccessRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessResponse);

            var service = new OAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                Mock.Of<IMemoryCache>(),
                config,
                _slackClientUnauthorizedFacadeMock.Object,
                _logger);

            // Act
            await service.HandleAuthorizationCodeCallbackAsync("foo", context);

            // Assert
            _vaultMock.Verify(x => x.SetSecretAsync(It.IsAny<string>(), It.IsAny<SecureString>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
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