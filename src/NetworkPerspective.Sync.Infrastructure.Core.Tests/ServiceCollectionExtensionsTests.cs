using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Common.Tests.Fixtures;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Core.Tests
{
    public class ServiceCollectionExtensionsTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly WireMockServer _wireMockServer;

        public ServiceCollectionExtensionsTests(MockedRestServerFixture slackClientFixture)
        {
            _httpClient = slackClientFixture.HttpClient;
            _wireMockServer = slackClientFixture.WireMockServer;
        }

        [Fact]
        public async Task GetServiceCollectionExtensions()
        {
            // Arrange
            const string scenarioName = "transientHttpErrors";
            const string stateOk = "state_ok";

            _wireMockServer
              .Given(Request.Create()
                .WithPath("/*"))
              .InScenario(scenarioName)
              .WillSetStateTo(stateOk, 2)
              .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.RequestTimeout));

            _wireMockServer
              .Given(Request.Create()
                .WithPath("/*"))
              .InScenario(scenarioName)
              .WhenStateIs(stateOk)
              .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithBodyAsJson(new ServiceResult
                {
                    ConnectorId = Guid.NewGuid(),
                    NetworkId = Guid.NewGuid()
                }));

            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "NetworkPerspectiveCore:BaseUrl", _httpClient.BaseAddress.ToString() },
                    { "NetworkPerspectiveCore:MaxInteractionsPerRequestCount", "100" },
                    { "NetworkPerspectiveCore:DataSourceIdName", "SlackId" },
                    { "NetworkPerspectiveCore:Resiliency:Retries:0", "00:00:00.010" },
                    { "NetworkPerspectiveCore:Resiliency:Retries:1", "00:00:00.010" }
                })
                .Build();


            services.AddNetworkPerspectiveCore(config.GetSection("NetworkPerspectiveCore"), Mock.Of<IHealthChecksBuilder>());

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var client = serviceProvider.GetRequiredService<ISyncHashedClient>();

            // Assert
            var result = await client.QueryAsync("foo");
            _wireMockServer.LogEntries.Count().Should().Be(3);

        }
    }
}