using System;
using System.Collections.Generic;
using System.Net.Http;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Moq;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Common.Tests.Fixtures
{
    public class InMemoryHostedServiceFixture<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        public SqliteUnitOfWorkFactory UnitOfWorkFactory { get; } = new SqliteUnitOfWorkFactory();

        public Mock<INetworkPerspectiveCore> NetworkPerspectiveCoreMock = new();
        public Mock<IVault> SecretRepositoryMock = new();

        public string ValidToken = "valid-token";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            var configOverride = new Dictionary<string, string>
            {
                { "Connector:Scheduler:UsePersistentStore", "false" },
                { "NLog:variables:minLevel", "FATAL"}
            };

            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(configOverride);
            });

            builder.ConfigureTestServices((Action<IServiceCollection>)(services =>
            {
                services.AddSingleton((IUnitOfWorkFactory)UnitOfWorkFactory);
                services.AddSingleton(Mock.Of<IDbInitializer>());

                services.AddSingleton(NetworkPerspectiveCoreMock.Object);
                services.AddSingleton(this.SecretRepositoryMock.Object);
            }));
        }

        public void Reset()
        {
            NetworkPerspectiveCoreMock.Reset();
            SecretRepositoryMock.Reset();
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);

            client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {ValidToken}");
        }
    }
}