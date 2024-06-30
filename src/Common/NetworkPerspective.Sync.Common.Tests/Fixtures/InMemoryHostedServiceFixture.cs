using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Moq;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Common.Tests.Fixtures
{
    public class InMemoryHostedServiceFixture<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        public SqliteUnitOfWorkFactory UnitOfWorkFactory { get; } = new SqliteUnitOfWorkFactory();

        public Mock<INetworkPerspectiveCore> NetworkPerspectiveCoreMock = new Mock<INetworkPerspectiveCore>();
        public Mock<ISecretRepositoryFactory> SecretRepositoryFactoryMock = new Mock<ISecretRepositoryFactory>();
        public Mock<ISecretRepository> SecretRepositoryMock = new Mock<ISecretRepository>();

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

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IUnitOfWorkFactory>(UnitOfWorkFactory);
                services.AddSingleton(Mock.Of<IDbInitializer>());

                services.AddSingleton(NetworkPerspectiveCoreMock.Object);
                services.AddSingleton(SecretRepositoryFactoryMock.Object);
            });
        }

        public void Reset()
        {
            NetworkPerspectiveCoreMock.Reset();
            SecretRepositoryFactoryMock.Reset();
            SecretRepositoryMock.Reset();

            SecretRepositoryFactoryMock
                .Setup(x => x.Create(It.IsAny<Uri>()))
                .Returns(SecretRepositoryMock.Object);
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);

            client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {ValidToken}");
        }
    }
}