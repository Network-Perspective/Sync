using System.Collections.Generic;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract;

namespace NetworkPerspective.Sync.Orchestrator.Tests;

public class OrchestratorServiceFixture : WebApplicationFactory<Program>
{
    public SqliteUnitOfWorkFactory UnitOfWorkFactory { get; } = new SqliteUnitOfWorkFactory();
    public Mock<ICore> NetworkPerspectiveCoreMock = new Mock<ICore>();


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        var configOverride = new Dictionary<string, string>
            {
                { "App:Scheduler:UsePersistentStore", "false" },
            };

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(configOverride);
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IUnitOfWorkFactory>(UnitOfWorkFactory);
            services.AddSingleton(Mock.Of<IDbInitializer>());

            services.AddSingleton(NetworkPerspectiveCoreMock.Object);
        });
    }
}