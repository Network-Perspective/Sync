using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Tests.Services
{
    public class MembersClientTests : IClassFixture<SlackClientFixture>
    {
        private readonly ISlackHttpClient _slackHttpClient;

        public MembersClientTests(SlackClientFixture slackClientFixture)
        {
            _slackHttpClient = slackClientFixture.SlackHttpClient;
        }

        [SkippableFact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldReturnNonEmptyLookupTable()
        {
            // Arrange
            var clientLogger = NullLogger<MembersClient>.Instance;
            var paginationLogger = NullLogger<CursorPaginationHandler>.Instance;

            var paginationHandler = new CursorPaginationHandler(paginationLogger);

            var slackClientFacade = new SlackClientBotScopeFacade(_slackHttpClient, paginationHandler);
            var membersClient = new MembersClient(clientLogger);

            try
            {
                // Act
                var result = await membersClient.GetEmployees(slackClientFacade, EmployeeFilter.Empty);

                // Assert
                result.GetAllInternal().Should().NotBeEmpty();
            }
            catch (ApiException exception)
            {
                Skip.If(exception.Message.Contains("invalid_auth"), "Please setup slack auth for slack api changes testing");

                throw;
            }
        }
    }
}