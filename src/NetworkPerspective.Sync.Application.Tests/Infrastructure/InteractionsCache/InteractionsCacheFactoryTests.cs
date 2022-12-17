using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Infrastructure.InteractionsCache
{
    public class InteractionsCacheFactoryTests
    {
        private readonly Mock<INetworkService> _networkServiceMock = new Mock<INetworkService>();
        private readonly IDataProtectionProvider _dataProtectionProvider = new EphemeralDataProtectionProvider();
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

        [Fact]
        public async Task ShouldCreateDurableCache()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var networkProperties = new NetworkProperties(true, null, true);

            _networkServiceMock
                .Setup(x => x.GetAsync<NetworkProperties>(networkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Network<NetworkProperties>.Create(networkId, networkProperties, DateTime.UtcNow));

            var factory = new InteractionsCacheFactory(_networkServiceMock.Object, _dataProtectionProvider, _loggerFactory);

            // Act
            var cache = await factory.CreateAsync(networkId);

            // Assert
            cache.Should().BeAssignableTo<InteractionsFileCache>();
        }

        [Fact]
        public async Task ShouldCreateVolatileCache()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var networkProperties = new NetworkProperties(true, null, false);

            _networkServiceMock
                .Setup(x => x.GetAsync<NetworkProperties>(networkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Network<NetworkProperties>.Create(networkId, networkProperties, DateTime.UtcNow));

            var factory = new InteractionsCacheFactory(_networkServiceMock.Object, _dataProtectionProvider, _loggerFactory);

            // Act
            var cache = await factory.CreateAsync(networkId);

            // Assert
            cache.Should().BeAssignableTo<InteractionsInMemoryCache>();
        }
    }
}
