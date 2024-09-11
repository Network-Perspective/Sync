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

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.MicrosoftAuth;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.MicrosoftAuth;

public class MicrosoftAuthServiceTests
{
    private readonly Mock<IVault> _vaultMock = new();
    private readonly Mock<IAuthStateKeyFactory> _authStateKeyFactoryMock = new();
    private readonly ILogger<MicrosoftAuthService> _logger = NullLogger<MicrosoftAuthService>.Instance;

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
                .Setup(x => x.GetSecretAsync(MicrosoftAuthService.MicrosoftClientBasicIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new MicrosoftAuthService(_vaultMock.Object, _authStateKeyFactoryMock.Object, cache, _logger);

            // Act
            var result = await service.StartAuthProcessAsync(new MicrosoftAuthProcess(connectorId, "worker-name", new Uri(redirectUrl), false));

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
            var authProcess = new MicrosoftAuthProcess(connectorId, "worker-name", callbackUri, true);

            var state = Guid.NewGuid().ToString();

            _authStateKeyFactoryMock
                .Setup(x => x.Create())
                .Returns(state);

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new MicrosoftAuthService(_vaultMock.Object, _authStateKeyFactoryMock.Object, cache, _logger);

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

            var service = new MicrosoftAuthService(_vaultMock.Object, _authStateKeyFactoryMock.Object, cache, _logger);

            // Act
            Func<Task> func = () => service.HandleCallbackAsync(Guid.NewGuid(), state);

            // Assert
            await func.Should().ThrowAsync<OAuthException>();
        }
    }

    // TODO: Test if tokens are sent to worker
}