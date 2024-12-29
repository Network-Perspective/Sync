using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Services;

public class CalendarClientTests(MicrosoftClientBasicFixture microsoftClientFixture) : IClassFixture<MicrosoftClientBasicFixture>
{
    private readonly ILogger<UsersClient> _usersClientlogger = NullLogger<UsersClient>.Instance;
    private readonly ILogger<CalendarClient> _calendarClientlogger = NullLogger<CalendarClient>.Instance;

    [Fact]
    [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
    public async Task ShouldSyncInteractions()
    {
        // Arrange
        var stream = new TestableInteractionStream();
        var usersClient = new UsersClient(microsoftClientFixture.Client, Mock.Of<IGlobalStatusCache>(), _usersClientlogger);

        var timeRange = new TimeRange(new DateTime(2023, 04, 10), new DateTime(2023, 04, 11));
        var syncContext = new SyncContext(Guid.NewGuid(), string.Empty, ConnectorConfig.Empty, [], new SecureString(), timeRange);
        var users = await usersClient.GetUsersAsync(syncContext);
        var employees = EmployeesMapper.ToEmployees(users, HashFunction.Empty, EmployeeFilter.Empty, true);

        var interactionFactory = new MeetingInteractionFactory(HashFunction.Empty, employees, NullLogger<MeetingInteractionFactory>.Instance);
        var calednarClient = new CalendarClient(microsoftClientFixture.Client, Mock.Of<IGlobalStatusCache>(), _calendarClientlogger);

        // Act
        await calednarClient.SyncInteractionsAsync(syncContext, stream, users.Select(x => x.Mail), interactionFactory);

        var interactions_1 = stream.SentInteractions.Where(x => x.Timestamp == new DateTime(2023, 04, 10, 06, 00, 00));
        interactions_1.Should().HaveCount(2);
        interactions_1.Should().OnlyContain(x => x.EventId == interactions_1.First().EventId);

        var interactions_2 = stream.SentInteractions.Where(x => x.Timestamp == new DateTime(2023, 04, 10, 07, 00, 00));
        interactions_2.Should().HaveCount(2);
        interactions_2.Should().OnlyContain(x => x.EventId == interactions_2.First().EventId);

        stream.SentInteractions.Should().HaveCount(4);
    }
}