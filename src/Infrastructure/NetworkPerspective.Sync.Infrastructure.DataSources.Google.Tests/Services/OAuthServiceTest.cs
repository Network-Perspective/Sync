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

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Clients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Model;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Services;

public class OAuthServiceTest
{
    private const string ClientId = "1234";

    private readonly Mock<IVault> _vaultMock = new();
    private readonly Mock<IAuthStateKeyFactory> _stateFactoryMock = new();
    private readonly Mock<IOAuthClient> _oAuthClientMock = new();
    private readonly ILogger<OAuthService> _logger = NullLogger<OAuthService>.Instance;

    public OAuthServiceTest()
    {
        _vaultMock.Reset();
        _stateFactoryMock.Reset();
        _oAuthClientMock.Reset();
    }

    public class StartAuthProcessAsync : OAuthServiceTest
    {
        [Fact]
        public async Task ShouldBuildCorrectAuthUri()
        {
            // Arrange
            var state = Guid.NewGuid().ToString();
            const string redirectUrl = "https://networkperspective.io:5001/callback";

            _stateFactoryMock
                .Setup(x => x.Create())
                .Returns(state);

            _vaultMock
                .Setup(x => x.GetSecretAsync(GoogleKeys.GoogleClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var service = new OAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                _oAuthClientMock.Object,
                cache,
                _logger);

            var connectorProperties = new Dictionary<string, string>
            { };
            var connectorInfo = new ConnectorContext(Guid.NewGuid(), "Google", connectorProperties);

            // Act
            var result = await service.InitializeOAuthAsync(new OAuthContext(connectorInfo, redirectUrl));

            // Assert
            result.AuthUri.Should().Contain("auth");
            result.AuthUri.Should().Contain($"client_id={ClientId}");
            //result.AuthUri.Should().Contain($"scope={string.Join('+', Scopes)}"); // TODO to be defined what we expect here
            result.AuthUri.Should().Contain($"redirect_uri={HttpUtility.UrlEncode(redirectUrl)}");
        }

        [Fact]
        public async Task ShouldPutStateToCache()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var callbackUri = "https://localhost:5001/callback";
            var connectorInfo = new ConnectorContext(connectorId, "Google", new Dictionary<string, string>());
            var context = new OAuthContext(connectorInfo, callbackUri);

            var state = Guid.NewGuid().ToString();

            _stateFactoryMock
                .Setup(x => x.Create())
                .Returns(state);

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new OAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                _oAuthClientMock.Object,
                cache,
                _logger);

            // Act
            var result = await service.InitializeOAuthAsync(context);

            // Assert
            cache.Get(state).Should().Be(context);
        }
    }

    public class HandleAuthorizationCodeCallback : OAuthServiceTest
    {
        [Fact]
        public async Task ShouldSetKeysInVault()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var callbackUri = "https://localhost:5001/callback";
            var code = Guid.NewGuid().ToString();
            var accessToken = Guid.NewGuid().ToString();
            var refreshToken = Guid.NewGuid().ToString();

            var connectorInfo = new ConnectorContext(connectorId, "Google", new Dictionary<string, string>());
            var context = new OAuthContext(connectorInfo, callbackUri);

            _oAuthClientMock
                .Setup(x => x.ExchangeCodeForTokenAsync(code, It.IsAny<string>(), It.IsAny<string>(), callbackUri, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse(accessToken, refreshToken));

            var service = new OAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                _oAuthClientMock.Object,
                Mock.Of<IMemoryCache>(),
                _logger);

            // Act
            await service.HandleAuthorizationCodeCallbackAsync(code, context);

            // Assert
            _vaultMock.Verify(x => x.SetSecretAsync(It.IsAny<string>(), It.IsAny<SecureString>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}