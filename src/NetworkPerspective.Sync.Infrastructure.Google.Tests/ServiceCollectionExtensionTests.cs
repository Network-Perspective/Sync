using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Application;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests
{
    public class ServiceCollectionExtensionTests
    {
        [Fact]
        public async Task ShouldRegisterRequiredServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Connector:Sync:DefaultSyncLookbackInDays", "5" },
                    { "Connector:Misc:DataSourceName", "GSuite" },
                    { "Infrastructure:Google:Domain", "networkperspective.io" },
                    { "Infrastructure:Google:ApplicationName", "app_connector" },
                    { "Infrastructure:Google:AdminEmail", "admin@networkperspective.io" }
                })
                .Build();

            serviceCollection.AddLogging();
            serviceCollection.AddTransient(x => Mock.Of<ISecretRepositoryFactory>());
            serviceCollection.AddSingleton(Mock.Of<IUnitOfWork>());
            serviceCollection.AddSingleton(Mock.Of<ISyncScheduler>());
            serviceCollection.AddSingleton(Mock.Of<IStatusLogger>());
            serviceCollection.AddSingleton(Mock.Of<IUnitOfWorkFactory>());
            serviceCollection.AddSingleton(Mock.Of<INetworkPerspectiveCore>());
            serviceCollection.AddApplication(config.GetSection("Connector"));

            // Act
            serviceCollection.AddGoogleDataSource(config.GetSection("Infrastructure:Google"));

            // Assert
            var services = serviceCollection.BuildServiceProvider();
            var factory = services.GetRequiredService<IDataSourceFactory>();
            var dataSource = await factory.CreateAsync(Guid.NewGuid());
            dataSource.Should().BeAssignableTo<GoogleFacade>();
        }
    }
}