using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Common.Tests.Factories;
using NetworkPerspective.Sync.Infrastructure.Core.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Core.Tests.Services
{
    public class InteractionsStreamTests
    {
        private readonly Mock<ISyncHashedClient> _clientMock = new Mock<ISyncHashedClient>();
        private readonly ILogger<InteractionsStream> _logger = NullLogger<InteractionsStream>.Instance;

        public InteractionsStreamTests()
        {
            _clientMock.Reset();
        }

        [Fact]
        public async Task ShouldUseClientToPush()
        {
            // Arrange
            var accessToken = "foo".ToSecureString();
            var config = CreateConfig(2);
            var interactions = InteractionFactory.CreateSet(10);

            var stream = new InteractionsStream(accessToken, _clientMock.Object, config, _logger, CancellationToken.None);

            // Act
            await stream.SendAsync(interactions);

            // Assert
            var expectedRequestsCount = Times.Exactly(interactions.Count / config.MaxInteractionsPerRequestCount);
            _clientMock.Verify(
                x => x.SyncInteractionsAsync(
                    It.Is<SyncHashedInteractionsCommand>(x => x.Interactions.Count == config.MaxInteractionsPerRequestCount),
                    It.IsAny<CancellationToken>()),
                expectedRequestsCount);
        }

        [Fact]
        public async Task ShouldFlushOnDispose()
        {
            // Arrange
            var accessToken = "foo".ToSecureString();
            var config = CreateConfig(10);
            var interactions = InteractionFactory.CreateSet(5);

            var stream = new InteractionsStream(accessToken, _clientMock.Object, config, _logger, CancellationToken.None);
            await stream.SendAsync(interactions);

            // Act
            await stream.DisposeAsync();

            // Assert
            _clientMock.Verify(
                x => x.SyncInteractionsAsync(
                    It.Is<SyncHashedInteractionsCommand>(x => x.Interactions.Count == interactions.Count),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ShouldDestroyTokenOnDispose()
        {
            // Arrange
            var accessToken = "foo".ToSecureString();
            var config = CreateConfig(10);

            var stream = new InteractionsStream(accessToken, _clientMock.Object, config, _logger, CancellationToken.None);

            // Act
            await stream.DisposeAsync();

            // Assert
            Action action = () => accessToken.ToSystemString();
            action.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public async Task ShouldNoThrowOnDispose()
        {
            // Arrange
            var accessToken = "foo".ToSecureString();
            var config = CreateConfig(10);
            var interactions = InteractionFactory.CreateSet(5);

            _clientMock
                .Setup(x => x.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var stream = new InteractionsStream(accessToken, _clientMock.Object, config, _logger, CancellationToken.None);
            await stream.SendAsync(interactions);

            // Act
            Func<Task> func = async () => await stream.DisposeAsync().AsTask();

            // Act Assert
            await func.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldStopOnCancelled()
        {
            // Arrange
            var accessToken = "foo".ToSecureString();
            var config = CreateConfig(2);
            var interactions = InteractionFactory.CreateSet(100);
            var invocationCount = 0;

            var cancellationTokenSource = new CancellationTokenSource();

            _clientMock
                .Setup(x => x.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>(), It.IsAny<CancellationToken>()))
                .Callback<SyncHashedInteractionsCommand, CancellationToken>((command, ct) =>
                {
                    invocationCount++;

                    if (invocationCount > 3)
                        cancellationTokenSource.Cancel();
                })
                .ReturnsAsync("bar");

            var stream = new InteractionsStream(accessToken, _clientMock.Object, config, _logger, cancellationTokenSource.Token);

            // Act
            await stream.SendAsync(interactions);

            // Assert
            var expectedRequestsCount = Times.Exactly(4);
            _clientMock.Verify(
                x => x.SyncInteractionsAsync(
                    It.IsAny<SyncHashedInteractionsCommand>(),
                    It.IsAny<CancellationToken>()),
                expectedRequestsCount);
        }

        private NetworkPerspectiveCoreConfig CreateConfig(int batchSize)
            => new NetworkPerspectiveCoreConfig
            {
                DataSourceIdName = "test",
                MaxInteractionsPerRequestCount = batchSize
            };


    }
}