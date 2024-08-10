using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client.HttpClients;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Tests
{
    public class SlackClientFacadeFactoryTests : IClassFixture<MockedRestServerFixture>
    {
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly CursorPaginationHandler _cursorPaginationHandler = new(NullLogger<CursorPaginationHandler>.Instance);
        private readonly MockedRestServerFixture _serverFixture;
        private readonly IOptions<Resiliency> _resiliencyOptions = Options.Create(new Resiliency
        {
            Retries = Enumerable
                .Range(0, 5)
                .Select(x => TimeSpan.FromMilliseconds(1))
                .ToArray()
        });

        public SlackClientFacadeFactoryTests(MockedRestServerFixture serverFixture)
        {
            _serverFixture = serverFixture;

            const string scenarioName = "transientHttpErrors";
            const string stateOk = "state_ok";

            var errorResponse = new SampleResponseWithError
            {
                IsOk = false,
                Error = SlackApiErrorCodes.InternalError,
            };

            var successResponse = new SampleResponseWithError
            {
                IsOk = true,
            };

            _serverFixture.WireMockServer.ResetLogEntries();
            _serverFixture.WireMockServer
                .Given(Request.Create())
                .InScenario(scenarioName)
                .WillSetStateTo(stateOk, 3)
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.InternalServerError)
                    .WithBodyAsJson(errorResponse));

            _serverFixture.WireMockServer
                .Given(Request.Create())
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
        public async Task ShouldCreateResilientClientThatEventuallyReturnResponse()
        {
            // Arrange
            var factory = new SlackClientFacadeFactory(_resiliencyOptions, _loggerFactory, _httpClientFactoryMock.Object, _cursorPaginationHandler);
            var client = factory.CreateUnauthorized();

            // Act
            var result = await client.AccessAsync(new OAuthAccessRequest());

            // Assert
            result.IsOk.Should().BeTrue();
            _serverFixture.WireMockServer.LogEntries.Should().HaveCountGreaterThan(1);
        }
    }
}