using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class HashingServiceTests
    {
        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public async Task ShouldBeDeterministic(string input)
        {
            // Arrange
            var hashingKey = Guid.NewGuid().ToString();
            using var hashingService = await CreateHashingServiceAsync(hashingKey);

            // Act
            var hash1 = hashingService.Hash(input);
            var hash2 = hashingService.Hash(input);

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public async Task ShouldBeThreadSafe()
        {
            // Arrange
            var hashingKey = Guid.NewGuid().ToString();
            var hashingService = await CreateHashingServiceAsync(hashingKey);

            // Act Assert
            Parallel.For(0, 10, i => hashingService.Hash(Guid.NewGuid().ToString()));
        }

        [Fact]
        public async Task ShouldReturnNullForNullInput()
        {
            // Arrange
            var hashingKey = Guid.NewGuid().ToString();
            var hashingService = await CreateHashingServiceAsync(hashingKey);

            // Act
            var hash1 = hashingService.Hash(null);

            // Assert
            hash1.Should().Be(null);
        }

        private async Task<IHashingService> CreateHashingServiceAsync(string hashingKey)
        {
            var networkId = Guid.NewGuid();
            var secretRepositoryMock = new Mock<ISecretRepository>();

            secretRepositoryMock
                .Setup(x => x.GetSecretAsync(string.Format(Keys.HashingKey, networkId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NetworkCredential(string.Empty, hashingKey).SecurePassword);

            var hashingServiceFactory = new HashingServiceFactory(NullLoggerFactory.Instance);
            return await hashingServiceFactory.CreateAsync(secretRepositoryMock.Object);
        }
    }
}