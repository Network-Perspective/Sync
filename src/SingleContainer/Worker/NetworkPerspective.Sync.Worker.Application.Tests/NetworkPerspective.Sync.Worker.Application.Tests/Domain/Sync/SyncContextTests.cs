using System;
using System.Security;

using Moq;

using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Domain.Sync
{
    public class SyncContextTests
    {
        [Fact]
        public void ShouldDisposeAllItems()
        {
            // Arrange
            var mock = new Mock<IDisposable>();
            var hashingServiceMock = new Mock<IHashingService>();
            var context = new SyncContext(Guid.NewGuid(), string.Empty, ConnectorConfig.Empty, [], new SecureString(), new TimeRange(DateTime.UtcNow, DateTime.Now), hashingServiceMock.Object);
            context.EnsureSet(() => mock.Object);

            // Act
            context.Dispose();

            // Assert
            mock.Verify(x => x.Dispose(), Times.Once);
            hashingServiceMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}