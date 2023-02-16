using System;
using System.Security;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Sync
{
    public class SyncContextTests
    {
        [Fact]
        public void ShouldDisposeAllItems()
        {
            // Arrange
            var mock = new Mock<IDisposable>();
            var hashingServiceMock = new Mock<IHashingService>();
            var context = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new NetworkProperties(), new SecureString(), new TimeRange(DateTime.UtcNow, DateTime.Now), Mock.Of<IStatusLogger>(), hashingServiceMock.Object);
            context.Set(mock.Object);

            // Act
            context.Dispose();

            // Assert
            mock.Verify(x => x.Dispose(), Times.Once);
            hashingServiceMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}