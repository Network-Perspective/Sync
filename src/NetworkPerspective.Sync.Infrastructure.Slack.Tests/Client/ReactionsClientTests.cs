using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client
{
    public class ReactionsClientTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly WireMockServer _wireMockServer;

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

            var reactionsClient = new ReactionsClient(_httpClient);

            // Act
            Func<Task<ReactionsGetResponse>> func = () => reactionsClient.GetAsync("foo", "bar");

            // Assert
            await func.Should().NotThrowAsync();
        }
    }
}