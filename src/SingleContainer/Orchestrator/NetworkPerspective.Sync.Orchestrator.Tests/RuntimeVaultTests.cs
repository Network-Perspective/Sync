using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Ovh;
using NetworkPerspective.Sync.Orchestrator.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests;

public class RuntimeVaultTests
{
    [Fact]
    public void LegacyVaultConfigurationRemainsAzureByDefault()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Vault:BaseUrl"] = "https://vault.example.invalid/",
            ["Vault:TestSecretName"] = "synthetic-health"
        }).Build().GetSection("Vault");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRuntimeVault(config, services.AddHealthChecks());
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IVault>().GetType().Name.Should().Be("AzureKeyVaultClient");
    }

    [Theory]
    [InlineData("OvhVault")]
    [InlineData("ovhvault")]
    public void ExplicitOvhSelectionUsesItsOwnSubsectionAndHealthCheck(string selectedProvider)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Vault:Provider"] = selectedProvider,
            ["Vault:BaseUrl"] = "https://ignored-azure.example.invalid/",
            ["Vault:OvhVault:BaseUrl"] = "https://vault.example.invalid/api/11111111-1111-1111-1111-111111111111",
            ["Vault:OvhVault:Token"] = "synthetic-only",
            ["Vault:OvhVault:MountPoint"] = "secret",
            ["Vault:OvhVault:TestSecretName"] = "synthetic-health"
        }).Build().GetSection("Vault");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRuntimeVault(config, services.AddHealthChecks());
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IVault>().GetType().Name.Should().Be("OvhVaultClient");
        provider.GetRequiredService<IOptions<OvhVaultConfig>>().Value.BaseUrl.Should()
            .Be("https://vault.example.invalid/api/11111111-1111-1111-1111-111111111111");
        provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations
            .Should().Contain(x => x.Name == "OVH Key-Vault");
    }

    [Fact]
    public void OvhSelectionDoesNotReadLegacyHcpOptions()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Vault:Provider"] = "OvhVault",
            ["Vault:HcpVault:BaseUrl"] = "https://vault.example.invalid/api/11111111-1111-1111-1111-111111111111",
            ["Vault:HcpVault:Token"] = "synthetic-only",
            ["Vault:HcpVault:MountPoint"] = "secret",
            ["Vault:HcpVault:TestSecretName"] = "synthetic-health"
        }).Build().GetSection("Vault");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRuntimeVault(config, services.AddHealthChecks());
        using var provider = services.BuildServiceProvider();
        Action resolve = () => provider.GetRequiredService<IVault>();
        resolve.Should().Throw<OptionsValidationException>();
    }

    [Theory]
    [InlineData("typo")]
    [InlineData("HcpVault")]
    public void UnknownProviderDoesNotSilentlyUseAzure(string selectedProvider)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Vault:Provider"] = selectedProvider
        }).Build().GetSection("Vault");
        var services = new ServiceCollection();
        Action act = () => services.AddRuntimeVault(config, services.AddHealthChecks());
        act.Should().Throw<InvalidOperationException>();
    }
}