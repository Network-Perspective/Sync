using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Tests.Fixtures;

public class OrchestratorServiceFixture : WebApplicationFactory<Program>
{
    private readonly string _validApiKey = Guid.NewGuid().ToString();

    public SqliteUnitOfWorkFactory UnitOfWorkFactory { get; } = new SqliteUnitOfWorkFactory();
    public Mock<IVault> VaultMock = new Mock<IVault>();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var configOverride = new Dictionary<string, string>
        {
            { "App:SyncScheduler:UsePersistentStore", "false" },
        };

        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(configOverride);
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);


        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IUnitOfWorkFactory>(UnitOfWorkFactory);
            services.AddSingleton(Mock.Of<IDbInitializer>());

            services.AddSingleton(VaultMock.Object);
        });

        VaultMock
            .Setup(x => x.GetSecretAsync(ApiKeyAuthOptions.DefaultApiKeyVaultKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_validApiKey.ToSecureString());
    }

    protected override void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _validApiKey);

        base.ConfigureClient(client);
    }
}