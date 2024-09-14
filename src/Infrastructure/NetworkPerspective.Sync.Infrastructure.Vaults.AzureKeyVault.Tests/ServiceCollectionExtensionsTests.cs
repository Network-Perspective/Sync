using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldRegisterRequiredServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Infrastructure:AzureKeyVault:BaseUrl", "https://nptestvault.vault.azure.net/" },
                { "Infrastructure:AzureKeyVault:TestSecretName", "test-key" }
            })
            .Build();

        serviceCollection.AddLogging();

        var healthCheckBuilder = serviceCollection.AddHealthChecks();

        // Act
        serviceCollection
            .AddAzureKeyVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        // Assert
        var services = serviceCollection.BuildServiceProvider();
        services.GetRequiredService<IVault>();
    }
}