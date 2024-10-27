using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Tests
{
    public class ThrottlingRetryTests
    {
        [Fact]
        public async Task RetryOnThrottlingResponse()
        {
            // Arrange
            const string scenarioName = "throttling";
            const string stateOk = "need_delay";

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Infrastructure:Slack:BaseUrl", "https://slack.com/api/" },
                })
                .Build();

            using var stub = WireMockServer.Start();
            stub
              .Given(Request.Create())
              .InScenario(scenarioName)
              .WillSetStateTo(stateOk, 2)
              .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.TooManyRequests)
                .WithHeader(HeaderNames.RetryAfter, "1"));

            stub
              .Given(Request.Create()
                .WithPath("/"))
              .InScenario(scenarioName)
              .WhenStateIs(stateOk)
              .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK));

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddSlack(config.GetSection("Infrastructure:Slack"), new ConnectorType { Name = "Slack", DataSourceId = "SlackId" });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            using var httpClient = serviceProvider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient(Consts.SlackApiHttpClientName);

            httpClient.BaseAddress = new Uri(stub.Urls[0]);

            // Act
            var response = await httpClient.GetAsync("/");

            // Assert
            stub.LogEntries.Count().Should().Be(3);
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}