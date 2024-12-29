using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Services.TasksStatuses;

public class StatusCacheTests
{
    [Fact]
    public async Task ShouldPersistStatusInGlobalCache()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddSingleton<IGlobalStatusCache, GlobalStatusCache>()
            .AddScoped<IConnectorContextAccessor, ConnectorContextAccessor>()
            .AddScoped<IScopedStatusCache, ScopedStatusCache>();

        var servicesProvider = services.BuildServiceProvider();

        var connectorId = Guid.NewGuid();
        var connectorContext = new ConnectorContext(connectorId, "foo", new Dictionary<string, string>());
        var status = new SingleTaskStatus("caption", "description", null);

        // Act
        using (var scope = servicesProvider.CreateScope())
        {
            var contextAccessor = scope.ServiceProvider.GetRequiredService<IConnectorContextAccessor>();
            contextAccessor.Context = connectorContext;
            var scopedStatusCache = scope.ServiceProvider.GetRequiredService<IScopedStatusCache>();
            await scopedStatusCache.SetStatusAsync(status);
        }

        // Assert
        using (var scope = servicesProvider.CreateScope())
        {
            var contextAccessor = scope.ServiceProvider.GetRequiredService<IConnectorContextAccessor>();
            contextAccessor.Context = connectorContext;
            var scopedStatusCache = scope.ServiceProvider.GetRequiredService<IScopedStatusCache>();
            var actualStatus = await scopedStatusCache.GetStatusAsync();
            actualStatus.Should().BeEquivalentTo(status);
        }
    }
}