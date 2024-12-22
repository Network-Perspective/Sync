using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Scheduler;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Scheduler;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldRegisterScheduler()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Connector:Scheduler:Sync:CronExpression", "0 0 0 * * ?" }
            })
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        // Act
        serviceCollection.AddScheduler(config.GetSection("Connector:Scheduler"), "foo");

        // Assert
        var provider = serviceCollection.BuildServiceProvider();
        provider.GetRequiredService<ISyncScheduler>();
    }
}