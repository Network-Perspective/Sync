using System;
using System.Security;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache;

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
            
            var context = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new SecureString(), DateTime.UtcNow, DateTime.Now);
            context.Set(mock.Object);

            // Act
            context.Dispose();

            // Assert
            mock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}