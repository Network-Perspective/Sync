using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
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
    public class ConversationsClientTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly WireMockServer _wireMockServer;
        private readonly ILogger<ConversationsClient> _logger = NullLogger<ConversationsClient>.Instance;

        public ConversationsClientTests(MockedRestServerFixture slackClientFixture)
        {
            _httpClient = slackClientFixture.HttpClient;
            _wireMockServer = slackClientFixture.WireMockServer;
        }

        [Fact]
        public async Task ShouldBeAbleToGetConversationsList()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingGet()
                    .WithPath("/conversations.list*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetConversationsList));

            var conversationsClient = new ConversationsClient(_httpClient, _logger);

            // Act
            Func<Task<ConversationsListResponse>> func = () => conversationsClient.GetListAsync(2);

            // Assert
            await func.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldBeAbleToGetConversationMembers()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingGet()
                    .WithPath("/conversations.members*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetConversationMembers));

            var conversationsClient = new ConversationsClient(_httpClient, _logger);

            // Act
            Func<Task<ConversationMembersResponse>> func = () => conversationsClient.GetConversationMembersAsync("foo", 10);

            // Assert
            await func.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldBeAbleToGetConversationHistory()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingGet()
                    .WithPath("/conversations.history*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetConversationMembers));

            var conversationsClient = new ConversationsClient(_httpClient, _logger);

            // Act
            Func<Task<ConversationHistoryResponse>> func = () => conversationsClient.GetConversationHistoryAsync("foo", 1, new DateTime(2020, 01, 01), new DateTime(2030, 01, 01));

            // Assert
            await func.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldBeAbleToGetReplies()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingGet()
                    .WithPath("/conversations.replies*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetReplies));

            var conversationsClient = new ConversationsClient(_httpClient, _logger);

            // Act
            Func<Task<ConversationRepliesResponse>> func = () => conversationsClient.GetRepliesAsync("foo", "123", "oldest", "latest", 1);

            // Assert
            await func.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldBeAbleToJoinConversation()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .UsingPost()
                    .WithPath("/conversations.join*"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(SampleResponse.GetReplies));

            var conversationsClient = new ConversationsClient(_httpClient, _logger);

            // Act
            Func<Task<JoinConversationResponse>> func = () => conversationsClient.JoinConversationAsync("foo");

            // Assert
            await func.Should().NotThrowAsync();
        }
    }
}