using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Persistence.Init;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Tests
{
    public class DbInitializerTests
    {
        [SkippableFact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShoudInitialize()
        {
            // Arrange
            var testcontainersBuilder = new TestcontainersBuilder<MsSqlTestcontainer>()
                .WithDatabase(new MsSqlTestcontainerConfiguration
                {
                    Password = "Pa$$w0rd",
                });

            await using var testcontainer = testcontainersBuilder.Build();

            try
            {
                await testcontainer
                    .StartAsync();
            } 
            catch (Exception e)
            {
                Skip.If(e is System.TimeoutException, "Setup docker for creating test containers");
            }

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:Database", testcontainer.ConnectionString + " TrustServerCertificate = True" }
                })
                .Build();

            var unitOfWorkFactory = new UnitOfWorkFactory(config);
            var initializer = new DbInitializer(unitOfWorkFactory, NullLogger<DbInitializer>.Instance);

            // Act Assert
            await initializer.InitializeAsync();
        }
    }
}