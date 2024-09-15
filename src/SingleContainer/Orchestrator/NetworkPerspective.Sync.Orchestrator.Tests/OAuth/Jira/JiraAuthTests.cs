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

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.OAuth.Jira;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;


namespace NetworkPerspective.Sync.Orchestrator.Tests.OAuth.Jira;

public class JiraAuthTests
{
    private const string ClientId = "1234";
    private static readonly string[] Scopes = ["scope1", "scope2"];

    private readonly Mock<IVault> _vaultMock = new();
    private readonly Mock<IAuthStateKeyFactory> _stateFactoryMock = new();
    private readonly Mock<IWorkerRouter> _workerRouter = new();
    private readonly ILogger<JiraAuthService> _logger = NullLogger<JiraAuthService>.Instance;

    public JiraAuthTests()
    {
        _vaultMock.Reset();
        _stateFactoryMock.Reset();
        _workerRouter.Reset();
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
                .Setup(x => x.GetSecretAsync(JiraAuthService.JiraClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClientId.ToSecureString());

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var service = new JiraAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _logger);

            // Act
            var result = await service.StartAuthProcessAsync(new JiraAuthProcess(Guid.NewGuid(), "worker-name", new Uri(redirectUrl)));

            // Assert
            result.JiraAuthUri.Should().Contain("authorize");
            result.JiraAuthUri.Should().Contain($"client_id={ClientId}");
            result.JiraAuthUri.Should().Contain($"scope={string.Join('+', Scopes)}");
            result.JiraAuthUri.Should().Contain($"redirect_uri={HttpUtility.UrlEncode(redirectUrl)}");
        }

        [Fact]
        public void ShouldPutStateToCache()
        {
            // Arrange
            var connectorId = Guid.NewGuid();
            var config = CreateDefaultOptions();
            var callbackUri = new Uri("https://localhost:5001/callback");
            var authProcess = new JiraAuthProcess(connectorId, "worker-name", callbackUri);

            var state = Guid.NewGuid().ToString();

            _stateFactoryMock
                .Setup(x => x.Create())
                .Returns(state);

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new JiraAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _logger);

            // Act
            var result = service.StartAuthProcessAsync(authProcess);

            // Assert
            cache.Get(state).Should().Be(authProcess);
        }
    }

    public class HandleAuthorizationCodeCallback : JiraAuthTests
    {
        [Fact]
        public async Task ShouldThrowAuthExceptionOnNonExistingState()
        {
            // Arrange
            var state = "non-exiting-state";

            var config = CreateDefaultOptions();

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var service = new JiraAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _logger);

            // Act
            Func<Task> func = () => service.HandleCallbackAsync("foo", state);

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
            var authState = new JiraAuthProcess(Guid.NewGuid(), workerName, new Uri("https://networkperspective.io/"));
            cache.Set(state, authState);

            var service = new JiraAuthService(
                _vaultMock.Object,
                _stateFactoryMock.Object,
                cache,
                _workerRouter.Object,
                config,
                _logger);

            // Act
            await service.HandleCallbackAsync("foo", state);

            // Assert
            _workerRouter.Verify(x => x.SetSecretsAsync(workerName, It.IsAny<IDictionary<string, SecureString>>()), Times.Once);
        }
    }

    private static IOptions<JiraConfig> CreateDefaultOptions()
    => Options.Create(new JiraConfig
    {
        BaseUrl = "https://network.perspective.io/",
        Auth = new JiraAuthConfig
        {
            Path = "authorize",
            Scopes = Scopes
        }
    });
}