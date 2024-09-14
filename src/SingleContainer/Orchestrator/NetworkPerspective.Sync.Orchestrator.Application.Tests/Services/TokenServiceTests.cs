using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IVault> _vaultMock = new();
        private readonly ILogger<TokenService> _logger = NullLogger<TokenService>.Instance;

        public TokenServiceTests()
        {
            _vaultMock.Reset();
        }

        public class AddOrReplace : TokenServiceTests
        {
            [Fact]
            public async Task ShouldSaveAccessToken()
            {
                // Arrange
                const string accessToken = "bar";
                var connectorId = Guid.NewGuid();

                var service = new TokenService(_vaultMock.Object, _logger);

                // Act
                await service.AddOrReplace(accessToken.ToSecureString(), connectorId);

                // Assert
                var expectedKey = $"np-token-{connectorId}";
                _vaultMock
                    .Verify(x => x.SetSecretAsync(
                        expectedKey,
                        It.Is<SecureString>(x => x.ToSystemString() == accessToken),
                        It.IsAny<CancellationToken>()));
            }
        }

        public class GetAccessToken : TokenServiceTests
        {
            [Fact]
            public async Task ShouldReturnAccessToken()
            {
                // Arrange
                const string accessToken = "bar";
                var connectorId = Guid.NewGuid();
                var key = $"np-token-{connectorId}";

                _vaultMock
                    .Setup(x => x.GetSecretAsync(key, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(accessToken.ToSecureString());

                var service = new TokenService(_vaultMock.Object, _logger);

                // Act
                var result = await service.GetAsync(connectorId);

                // Assert
                result.ToSystemString().Should().Be(accessToken);
            }
        }

        public class EnsureRemoved : TokenServiceTests
        {
            [Fact]
            public async Task ShouldNotThrowOnNonExistingSecret()
            {
                // Arrange
                var connectorId = Guid.NewGuid();
                var key = $"np-token-{connectorId}";

                _vaultMock
                    .Setup(x => x.GetSecretAsync(key, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new VaultException("message", new Exception()));

                var service = new TokenService(_vaultMock.Object, _logger);
                Func<Task> func = () => service.EnsureRemovedAsync(connectorId);

                // Act Assert
                await func.Should().NotThrowAsync();
            }
        }
    }
}