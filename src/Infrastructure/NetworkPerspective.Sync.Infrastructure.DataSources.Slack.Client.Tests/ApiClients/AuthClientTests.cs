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
    public class AuthClientTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly WireMockServer _wireMockServer;
        private readonly ILogger<SlackHttpClient> _logger = NullLogger<SlackHttpClient>.Instance;

        public AuthClientTests(MockedRestServerFixture slackClientFixture)
        {
            _httpClient = slackClientFixture.HttpClient;
            _wireMockServer = slackClientFixture.WireMockServer;
        }

        [Fact]
        public async Task ShouldBeAbleToGetTeamsList()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingGet()
                    .WithPath("/auth.teams.list*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetConversationsList));

            var authClient = new AuthClient(new SlackHttpClient(_httpClient, _logger));

            // Act
            Func<Task<TeamsListResponse>> func = () => authClient.GetTeamsListAsync(2);

            // Assert
            await func.Should().NotThrowAsync();
        }
    }
}