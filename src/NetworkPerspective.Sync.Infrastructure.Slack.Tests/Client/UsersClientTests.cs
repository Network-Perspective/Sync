using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client
{
    public class UsersClientTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly WireMockServer _wireMockServer;
        private readonly ILogger<UsersClient> _logger = NullLogger<UsersClient>.Instance;

        public UsersClientTests(MockedRestServerFixture slackClientFixture)
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
                    .UsingGet()
                    .WithPath("/users.list*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetUsersList));

            var usersClient = new UsersClient(_httpClient, _logger);

            // Act
            Func<Task<UsersListResponse>> func = () => usersClient.GetListAsync(2);

            // Assert
            await func.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldBeAbleToGetUsersConversations()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingGet()
                    .WithPath("/users.conversations*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetUsersConversations));

            var usersClient = new UsersClient(_httpClient, _logger);

            // Act
            Func<Task<UsersConversationsResponse>> func = () => usersClient.GetConversationsAsync("foo");

            // Assert
            await func.Should().NotThrowAsync();
        }
    }
}