using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.ApiClients;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Tests.ApiClients
{
    public class OAuthClientTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly WireMockServer _wireMockServer;

        public OAuthClientTests(MockedRestServerFixture slackClientFixture)
        {
            _httpClient = slackClientFixture.HttpClient;
            _wireMockServer = slackClientFixture.WireMockServer;
        }

        [Fact]
        public async Task ShouldBeAbleToGetUsersList()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingPost()
                    .WithPath("/oauth.v2.access*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.Access));


            var oAuthClient = new OAuthClient(new SlackHttpClient(_httpClient, NullLogger<SlackHttpClient>.Instance));

            // Act
            Func<Task<OAuthAccessResponse>> func = () => oAuthClient.AccessAsync(new OAuthAccessRequest());

            // Assert
            await func.Should().NotThrowAsync();
        }
    }
}