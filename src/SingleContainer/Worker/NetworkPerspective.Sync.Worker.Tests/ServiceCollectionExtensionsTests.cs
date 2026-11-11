using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Vaults.HashiCorpVault;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Tests;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData("AzureKeyVault")]
    [InlineData("azurekeyvault")]
    public void ShouldUseAzureKeyVault(string sectionName)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"Infrastructure:{sectionName}:BaseUrl", "https://networkperspective.io/" },
                { $"Infrastructure:{sectionName}:TestSecretName", "test-key" }
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

    [Theory]
    [InlineData("HcpVault")]
    [InlineData("hcpvault")]
    public void ShouldUseHcpVault(string sectionName)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"Infrastructure:{sectionName}:BaseUrl", "https://networkperspective.io/" },
                { $"Infrastructure:{sectionName}:TestSecretName", "test-key" }
            })
            .Build();


        var healthCheckBuilder = Mock.Of<IHealthChecksBuilder>();

        services.AddVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var vault = serviceProvider.GetRequiredService<IVault>();

        // Assert
        vault.GetType().Name.Should().Be("HcpVaultClient");
        var options = serviceProvider.GetRequiredService<IOptions<HcpVaultConfig>>().Value;
        options.BaseUrl.Should().Be("https://networkperspective.io/");
        options.TestSecretName.Should().Be("test-key");
        services.Should().NotContain(x => x.ServiceType == typeof(IValidateOptions<HcpVaultConfig>));
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

    [Theory]
    [InlineData("GoogleSecretManager")]
    [InlineData("googlesecretmanager")]
    public void ShouldUseGoogleSecretManager(string sectionName)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"Infrastructure:{sectionName}:ProjectId", "42" },

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

    [Theory]
    [InlineData("AmazonSecretsManager")]
    [InlineData("amazonsecretsmanager")]
    public void ShouldUseAmazonSecretsManager(string sectionName)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"Infrastructure:{sectionName}:Region", "eu-central-1" },
                { $"Infrastructure:{sectionName}:SecretsPrefix", "test/connectors" },

            })
            .Build();

        var healthCheckBuilder = Mock.Of<IHealthChecksBuilder>();

        services.AddVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var vault = serviceProvider.GetRequiredService<IVault>();

        // Assert
        vault.GetType().Name.Should().Be("AmazonSecretsManagerClient");
    }

    public static IEnumerable<object[]> PrimaryProviderPairs()
    {
        var providers = new[] { "AzureKeyVault", "HcpVault", "OvhVault", "GoogleSecretManager", "AmazonSecretsManager" };
        for (var i = 0; i < providers.Length; i++)
        {
            for (var j = i + 1; j < providers.Length; j++)
            {
                yield return new object[] { providers[i], providers[j] };
                yield return new object[] { providers[i], providers[j].ToLowerInvariant() };
            }
        }
    }

    [Theory]
    [MemberData(nameof(PrimaryProviderPairs))]
    public void ShouldRejectMultiplePrimaryProvidersBeforeRegisteringAnyVault(string first, string second)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            [$"{first}:BaseUrl"] = "https://synthetic.example.invalid/",
            [$"{second}:BaseUrl"] = "https://synthetic.example.invalid/"
        }).Build();
        var services = new ServiceCollection();
        var healthChecks = services.AddHealthChecks();

        Action register = () => services.AddVault(config, healthChecks);

        register.Should().Throw<InvalidVaultConfigurationException>()
            .WithMessage("Only one primary vault provider may be configured.");
        services.Should().NotContain(x => x.ServiceType == typeof(IVault));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ExternalAzureKeyVault")]
    [InlineData("UnknownVault")]
    public void ShouldThrowWithoutPrimaryProvider(string otherSection)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([])
            .Build();

        var healthCheckBuilder = Mock.Of<IHealthChecksBuilder>();

        if (otherSection is not null)
            config[$"Infrastructure:{otherSection}:BaseUrl"] = "https://synthetic.example.invalid/";

        Func<IServiceCollection> func = () => services.AddVault(config.GetSection("Infrastructure"), healthCheckBuilder);

        // Act Assert
        func.Should().Throw<InvalidVaultConfigurationException>();
    }


}