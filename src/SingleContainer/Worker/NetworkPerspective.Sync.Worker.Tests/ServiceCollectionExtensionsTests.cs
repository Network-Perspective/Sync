using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldUseAzureKeyVault()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Infrastructure:AzureKeyVault:BaseUrl", "https://networkperspective.io/" },
                { "Infrastructure:AzureKeyVault:TestSecretName", "test-key" }
            })
            .Build();


        var healthCheckBuilder = Mock.Of<IHealthChecksBuilder>();

        services.AddVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var vault = serviceProvider.GetRequiredService<IVault>();

        // Assert
        vault.GetType().Name.Should().Be("AzureKeyVaultClient");
    }

    [Fact]
    public void ShouldUseHcpVault()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Infrastructure:HcpVault:BaseUrl", "https://networkperspective.io/" },
                { "Infrastructure:HcpVault:TestSecretName", "test-key" }
            })
            .Build();


        var healthCheckBuilder = Mock.Of<IHealthChecksBuilder>();

        services.AddVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var vault = serviceProvider.GetRequiredService<IVault>();

        // Assert
        vault.GetType().Name.Should().Be("HcpVaultClient");
    }

    [Fact]
    public void ShouldUseExternalAzureKeyVault()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Infrastructure:AzureKeyVault:BaseUrl", "https://networkperspective.io/" },
                { "Infrastructure:AzureKeyVault:TestSecretName", "test-key" },
                { "Infrastructure:ExternalAzureKeyVault:BaseUrl", "https://ext.networkperspective.io/" },
                { "Infrastructure:ExternalAzureKeyVault:TestSecretName", "test-key" },
            })
            .Build();

        var healthCheckBuilder = Mock.Of<IHealthChecksBuilder>();

        services.AddVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var vault = serviceProvider.GetRequiredService<IVault>();

        // Assert
        vault.GetType().Name.Should().Be("ExternalAzureKeyVaultClient");
    }

    [Fact]
    public void ShouldUseGoogleSecretManager()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Infrastructure:GoogleSecretManager:ProjectId", "42" },

            })
            .Build();

        var healthCheckBuilder = Mock.Of<IHealthChecksBuilder>();

        services.AddVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var vault = serviceProvider.GetRequiredService<IVault>();

        // Assert
        vault.GetType().Name.Should().Be("GoogleSecretManagerClient");
    }

    [Fact]
    public void ShouldThrowOnNotConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([])
            .Build();

        var healthCheckBuilder = Mock.Of<IHealthChecksBuilder>();

        Func<IServiceCollection> func = () => services.AddVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        // Act Assert
        func.Should().Throw<InvalidVaultConfigurationException>();
    }


}