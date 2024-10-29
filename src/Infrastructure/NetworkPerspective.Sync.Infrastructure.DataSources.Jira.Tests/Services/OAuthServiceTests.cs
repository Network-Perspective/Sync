using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Services;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;


namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Tests.Services;

public class JiraAuthTests
{
    private const string ClientId = "1234";
    private static readonly string[] Scopes = ["scope1", "scope2"];

    private readonly Mock<IVault> _vaultMock = new();
    private readonly Mock<IAuthStateKeyFactory> _stateFactoryMock = new();
    private readonly Mock<IJiraUnauthorizedFacade> _jiraClientMock = new();
    private readonly ILogger<OAuthService> _logger = NullLogger<OAuthService>.Instance;

    public JiraAuthTests()
    {
        _vaultMock.Reset();
        _stateFactoryMock.Reset();
    }


    public class StartAuthProcessAsync : JiraAuthTests
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
                .Setup(x => x.GetSecretAsync(JiraKeys.JiraClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var service = new OAuthService(
                _vaultMock.Object,
                _jiraClientMock.Object,
                _stateFactoryMock.Object,
                cache,
                config,
                _logger);

            var connectorProperties = new Dictionary<string, string>
            { };
            var connectorInfo = new ConnectorInfo(Guid.NewGuid(), "Jira", connectorProperties);

            // Act
            var result = await service.InitializeOAuthAsync(new OAuthContext(connectorInfo, redirectUrl));

            // Assert
            result.AuthUri.Should().Contain("authorize");
            result.AuthUri.Should().Contain($"client_id={ClientId}");
            result.AuthUri.Should().Contain($"scope={string.Join('+', Scopes)}");
            result.AuthUri.Should().Contain($"redirect_uri={HttpUtility.UrlEncode(redirectUrl)}");
        }

        [Fact]
        public async Task ShouldPutStateToCache()
        {
            // Arrange
            var config = CreateDefaultOptions();

            var connectorId = Guid.NewGuid();
            var callbackUri = "https://localhost:5001/callback";
            var connectorInfo = new ConnectorInfo(connectorId, "Slack", new Dictionary<string, string>());
            var context = new OAuthContext(connectorInfo, callbackUri);

            var state = Guid.NewGuid().ToString();

            _stateFactoryMock
                .Setup(x => x.Create())
                .Returns(state);

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new OAuthService(
                _vaultMock.Object,
                _jiraClientMock.Object,
                _stateFactoryMock.Object,
                cache,
                config,
                _logger);

            // Act
            var result = await service.InitializeOAuthAsync(context);

            // Assert
            cache.Get(state).Should().Be(context);
        }
    }

    public class HandleAuthorizationCodeCallback : JiraAuthTests
    {
        [Fact]
        public async Task ShouldSetKeysInVault()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var callbackUri = "https://localhost:5001/callback";
            var code = Guid.NewGuid().ToString();

            var config = CreateDefaultOptions();

            var connectorInfo = new ConnectorInfo(connectorId, "Slack", new Dictionary<string, string>());
            var context = new OAuthContext(connectorInfo, callbackUri);

            _jiraClientMock
                .Setup(x => x.ExchangeCodeForTokenAsync(code, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OAuthTokenResponse { AccessToken = "access-token", RefreshToken = "refresh-token" });

            var service = new OAuthService(
                _vaultMock.Object,
                _jiraClientMock.Object,
                _stateFactoryMock.Object,
                Mock.Of<IMemoryCache>(),
                config,
                _logger);

            // Act
            await service.HandleAuthorizationCodeCallbackAsync(code, context);

            // Assert
            _vaultMock.Verify(x => x.SetSecretAsync(It.IsAny<string>(), It.IsAny<SecureString>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }

    private static IOptions<JiraConfig> CreateDefaultOptions()
    => Options.Create(new JiraConfig
    {
        BaseUrl = "https://network.perspective.io/",
        Auth = new JiraAuthConfig
        {
            BaseUrl = "https://networkperspective.io/",
            Path = "authorize",
            Scopes = Scopes
        }
    });
}