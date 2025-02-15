using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.Auths;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Worker.Application;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldUseUserOAuthServiceOnUseUserToken()
    {
        // Arrange
        var microsoftConnectorType = new ConnectorType { Name = "Office365", DataSourceId = "Office365Id" };
        var serviceProvider = CreateServiceProvider(microsoftConnectorType);
        var connectorContextAccessor = serviceProvider.GetRequiredService<IConnectorContextAccessor>();
        var props = new Dictionary<string, string>
        {
            { nameof(ConnectorProperties.UseUserToken), true.ToString() }
        };
        connectorContextAccessor.Context = new ConnectorContext(Guid.NewGuid(), microsoftConnectorType.Name, props);

        // Act
        var result = serviceProvider.GetKeyedService<IOAuthService>(microsoftConnectorType.GetKeyOf<IOAuthService>());

        // Assert
        Assert.IsAssignableFrom<UserOAuthService>(result);
    }

    [Fact]
    public void ShouldUseAdminScopedOAuthServiceOnUseUserToken()
    {
        // Arrange
        var microsoftConnectorType = new ConnectorType { Name = "Office365", DataSourceId = "Office365Id" };
        var serviceProvider = CreateServiceProvider(microsoftConnectorType);
        var connectorContextAccessor = serviceProvider.GetRequiredService<IConnectorContextAccessor>();
        var props = new Dictionary<string, string>
        {
            { nameof(ConnectorProperties.UseUserToken), false.ToString() }
        };
        connectorContextAccessor.Context = new ConnectorContext(Guid.NewGuid(), microsoftConnectorType.Name, props);

        // Act
        var result = serviceProvider.GetKeyedService<IOAuthService>(microsoftConnectorType.GetKeyOf<IOAuthService>());

        // Assert
        Assert.IsAssignableFrom<AdminConsentOAuthService>(result);
    }

    private static ServiceProvider CreateServiceProvider(ConnectorType microsoftConnectorType)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddLogging()
            .AddSingleton(Mock.Of<IVault>())
            .AddWorkerApplication([microsoftConnectorType])
            .AddMicrosoft(config, microsoftConnectorType);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider;
    }
}