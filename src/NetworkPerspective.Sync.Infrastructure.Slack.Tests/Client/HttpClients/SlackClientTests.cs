using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using Moq;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client.HttpClients
{
    public class SlackClientTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly MockedRestServerFixture _serverFixture;
        private readonly IOptions<Resiliency> _resiliencyOptions = Options.Create(new Resiliency
        {
            Retries = Enumerable
                .Range(0, 5)
                .Select(x => TimeSpan.FromMilliseconds(1))
                .ToArray()
        });

        public SlackClientTests(MockedRestServerFixture serverFixture)
        {
            _serverFixture = serverFixture;

            const string scenarioName = "transientHttpErrors";
            const string stateOk = "state_ok";

            var errorResponse = new SampleResponse
            {
                IsOk = false,
                Error = SlackApiErrorCodes.InternalError,
            };

            var successResponse = new SampleResponse
            {
                IsOk = true,
            };

            _serverFixture.WireMockServer.ResetLogEntries();

            _serverFixture.WireMockServer
                .Given(Request.Create()
                    .WithPath("/*"))
                .InScenario(scenarioName)
                .WillSetStateTo(stateOk, 3)
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.InternalServerError)
                    .WithBodyAsJson(errorResponse));

            _serverFixture.WireMockServer
                .Given(Request.Create()
                    .WithPath("/*"))
                .InScenario(scenarioName)
                .WhenStateIs(stateOk)
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBodyAsJson(successResponse));

            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(_serverFixture.HttpClient);
        }

        [Fact]
        public async Task ShouldEventuallyReturnResponse()
        {
            // Arrange
            var factory = new SlackHttpClientFactory(_resiliencyOptions, _loggerFactory, _httpClientFactoryMock.Object);
            var client = factory.Create();

            // Act
            var result = await client.GetAsync<SampleResponse>("/");

            // Assert
            result.IsOk.Should().BeTrue();
            _serverFixture.WireMockServer.LogEntries.Should().HaveCountGreaterThan(1);
        }

        [Fact]
        public async Task ShouldUseToken()
        {
            // Arrange
            const string token = "token";

            var factory = new SlackHttpClientFactory(_resiliencyOptions, _loggerFactory, _httpClientFactoryMock.Object);
            var client = factory.CreateWithToken(token.ToSecureString());

            // Act
            var result = await client.PostAsync<SampleResponse>("/");

            // Assert
            _serverFixture
                .WireMockServer
                .LogEntries
                .Last()
                .RequestMessage
                .Headers
                .Single(x => x.Key == HeaderNames.Authorization)
                .Value
                .Single()
                .Should()
                .Contain(token);
        }
    }
}
