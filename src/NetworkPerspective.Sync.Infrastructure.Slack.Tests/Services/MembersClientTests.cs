using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Services
{
    public class MembersClientTests : IClassFixture<SlackClientFixture>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MembersClientTests(SlackClientFixture slackClientFixture)
        {
            _httpClientFactory = slackClientFixture.HttpClientFactory;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldReturnNonEmptyLookupTable()
        {
            // Arrange
            var clientLogger = NullLogger<MembersClient>.Instance;
            var paginationLogger = NullLogger<CursorPaginationHandler>.Instance;

            var paginationHandler = new CursorPaginationHandler(paginationLogger);

            var slackClientFacade = new SlackClientFacade(_httpClientFactory, paginationHandler);
            var membersClient = new MembersClient(clientLogger);

            // Act
            var result = await membersClient.GetEmployees(slackClientFacade, EmailFilter.Empty);

            // Assert
            result.GetAllInternal().Should().NotBeEmpty();
        }
    }
}