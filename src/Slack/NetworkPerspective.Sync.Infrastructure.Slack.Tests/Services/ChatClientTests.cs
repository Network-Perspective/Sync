using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Models;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Services
{
    public class ChatClientTests : IClassFixture<SlackClientFixture>
    {
        private readonly ISlackHttpClient _slackHttpClient;

        public ChatClientTests(SlackClientFixture slackClientFixture)
        {
            _slackHttpClient = slackClientFixture.SlackHttpClient;
        }

        [SkippableFact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldReturnNonEmptyInteractionList()
        {
            // Arrange
            const string existingEmail = "maciej@networkperspective.io";
            var timeRange = new TimeRange(new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc), DateTime.UtcNow);
            var paginationHandler = new CursorPaginationHandler(NullLogger<CursorPaginationHandler>.Instance);
            var chatclient = new ChatClient(Mock.Of<ITasksStatusesCache>(), NullLogger<ChatClient>.Instance);

            var slackClientFacade = new SlackClientBotScopeFacade(_slackHttpClient, paginationHandler);
            var stream = new TestableInteractionStream();

            var employees = new List<Employee>()
                .Add(existingEmail);

            var employeesCollection = new EmployeeCollection(employees, null);

            var interactionFactory = new InteractionFactory((x) => $"{x}_hashed", employeesCollection);

            try
            {
                // Act
                await chatclient.SyncInteractionsAsync(stream, slackClientFacade, Guid.NewGuid(), interactionFactory, timeRange);

                // Assert
                stream.SentInteractions.Count.Should().BePositive();
            }
            catch (ApiException exception)
            {
                Skip.If(exception.Message.Contains("invalid_auth"), "Please setup slack auth for slack api changes testing");

                throw;
            }
        }
    }
}