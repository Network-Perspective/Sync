using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client
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


            var oAuthClient = new OAuthClient(_httpClient, NullLogger<OAuthClient>.Instance);

            // Act
            Func<Task<OAuthAccessResponse>> func = () => oAuthClient.AccessAsync(new OAuthAccessRequest());

            // Assert
            await func.Should().NotThrowAsync();
        }
    }
}