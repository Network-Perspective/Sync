using System;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class TasksStatusesCacheTests
    {
        [Fact]
        public async Task ShouldPersistStatusForNetwork()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var status = new SingleTaskStatus("First task", "This is super important task", 33.3);
            var cache = new TasksStatusesCache();

            // Act
            await cache.SetStatusAsync(networkId, status);

            // Assert
            (await cache.GetStatusAsync(networkId)).Should().BeEquivalentTo(status);
        }
    }
}