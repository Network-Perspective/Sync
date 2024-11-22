using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests;

public class ServiceCollectionExtensionTests
{
    [Fact]
    public void ShouldRegisterRequiredServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Connector:Sync:DefaultSyncLookbackInDays", "5" },
                { "Infrastructure:Google:ApplicationName", "app_connector" },
                { "Infrastructure:Google:AdminEmail", "admin@networkperspective.io" }
            })
            .Build();

        serviceCollection.AddLogging();
        serviceCollection.AddTransient(x => Mock.Of<IVault>());
        serviceCollection.AddSingleton(Mock.Of<INetworkPerspectiveCore>());
        serviceCollection.AddSingleton(Mock.Of<IOrchestratorHubClient>());
        var connectorType = new ConnectorType { Name = "Google", DataSourceId = "GSuiteId" };
        serviceCollection.AddWorkerApplication(config.GetSection("Connector"), [connectorType]);

        // Act
        serviceCollection.AddGoogle(config.GetSection("Infrastructure:Google"), connectorType);

        // Assert
        var services = serviceCollection.BuildServiceProvider();
        var timeRange = new TimeRange(DateTime.UtcNow, DateTime.UtcNow);
        var syncContext = new SyncContext(Guid.NewGuid(), "Google", ConnectorConfig.Empty, [], "".ToSecureString(), timeRange);
        services.GetRequiredService<ISyncContextAccessor>().SyncContext = syncContext;
        var dataSource = services.GetRequiredKeyedService<IDataSource>(connectorType.GetKeyOf<IDataSource>());
        dataSource.Should().BeAssignableTo<GoogleFacade>();
    }
}