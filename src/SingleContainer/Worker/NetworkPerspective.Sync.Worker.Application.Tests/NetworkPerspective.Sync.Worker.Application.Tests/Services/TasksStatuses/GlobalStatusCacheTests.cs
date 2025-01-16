using System;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Services.TasksStatuses;

public class GlobalStatusCacheTests
{
    [Fact]
    public async Task ShouldPersistStatusForNetwork()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var status = SingleTaskStatus.New("First task", "This is super important task", 33.3);
        var cache = new GlobalStatusCache();

        // Act
        await cache.SetStatusAsync(connectorId, status);

        // Assert
        (await cache.GetStatusAsync(connectorId)).Should().BeEquivalentTo(status);
    }
}