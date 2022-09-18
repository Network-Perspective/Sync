using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Tests
{
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
                    { "ConnectionStrings:Database", "foo" }
                })
                .Build();

            serviceCollection.AddSingleton(config as IConfiguration);
            var healthCheckBuilder = serviceCollection.AddHealthChecks();

            // Act
            serviceCollection.AddPersistence(healthCheckBuilder);

            // Assert
            var services = serviceCollection.BuildServiceProvider();
            services.GetRequiredService<IUnitOfWorkFactory>();
            services.GetRequiredService<IUnitOfWork>();
        }
    }
}