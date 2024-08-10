using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Tests;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.ApiClients;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Tests.ApiClients
{
    public class ReactionsClientTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly WireMockServer _wireMockServer;
        private readonly ILogger<SlackHttpClient> _logger = NullLogger<SlackHttpClient>.Instance;

        public ReactionsClientTests(MockedRestServerFixture slackClientFixture)
        {
            _httpClient = slackClientFixture.HttpClient;
            _wireMockServer = slackClientFixture.WireMockServer;
        }

        [Fact]
        public async Task ShouldBeAbleToGetReactions()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingGet()
                    .WithPath("/reactions.get*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetReactions));

            var reactionsClient = new ReactionsClient(new SlackHttpClient(_httpClient, _logger));

            // Act
            Func<Task<ReactionsGetResponse>> func = () => reactionsClient.GetAsync("foo", "bar");

            // Assert
            await func.Should().NotThrowAsync();
        }
    }
}