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

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Services;

public class MicrosoftAuthServiceTests
{
    private readonly Mock<IVault> _vaultMock = new();
    private readonly Mock<IAuthStateKeyFactory> _authStateKeyFactoryMock = new();
    private readonly ILogger<OAuthService> _logger = NullLogger<OAuthService>.Instance;

    public MicrosoftAuthServiceTests()
    {
        _vaultMock.Reset();
        _authStateKeyFactoryMock.Reset();
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

            _authStateKeyFactoryMock
                .Setup(x => x.Create())
                .Returns(state);

            _vaultMock
                .Setup(x => x.GetSecretAsync(OAuthService.MicrosoftClientBasicIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new OAuthService(_vaultMock.Object, _authStateKeyFactoryMock.Object, cache, _logger);

            var connectorProperties = new Dictionary<string, string>
            {
                { "SyncMsTeams", "false" }
            };
            var connectorInfo = new ConnectorInfo(Guid.NewGuid(), "Office365", connectorProperties);

            // Act
            var result = await service.InitializeOAuthAsync(new OAuthContext(connectorInfo, redirectUrl));

            // Assert
            var resultUri = new Uri(result.AuthUri);
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
            var callbackUri = "https://localhost:5001/callback";
            var connectorInfo = new ConnectorInfo(connectorId, "Office365", new Dictionary<string, string>());
            var context = new OAuthContext(connectorInfo, callbackUri);

            var state = Guid.NewGuid().ToString();

            _authStateKeyFactoryMock
                .Setup(x => x.Create())
                .Returns(state);

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new OAuthService(_vaultMock.Object, _authStateKeyFactoryMock.Object, cache, _logger);

            // Act
            var result = service.InitializeOAuthAsync(context);

            // Assert
            cache.Get(state).Should().Be(context);
        }
    }

    public class HandleCallback : MicrosoftAuthServiceTests
    {
        [Fact]
        public async Task ShouldSetKeysInVault()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var callbackUri = "https://localhost:5001/callback";

            var connectorInfo = new ConnectorInfo(connectorId, "Office365", new Dictionary<string, string>());
            var context = new OAuthContext(connectorInfo, callbackUri);

            var service = new OAuthService(_vaultMock.Object, _authStateKeyFactoryMock.Object, Mock.Of<IMemoryCache>(), _logger);

            // Act
            await service.HandleAuthorizationCodeCallbackAsync(Guid.NewGuid().ToString(), context);

            // Assert
            _vaultMock.Verify(x => x.SetSecretAsync(It.IsAny<string>(), It.IsAny<SecureString>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }
    }
}