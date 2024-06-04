using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Models;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Services
{
    public class CalendarClientTests : IClassFixture<MicrosoftClientBasicFixture>
    {
        private readonly MicrosoftClientBasicFixture _microsoftClientFixture;
        private readonly ILogger<UsersClient> _usersClientlogger = NullLogger<UsersClient>.Instance;
        private readonly ILogger<CalendarClient> _calendarClientlogger = NullLogger<CalendarClient>.Instance;

        public CalendarClientTests(MicrosoftClientBasicFixture microsoftClientFixture)
        {
            _microsoftClientFixture = microsoftClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldSyncInteractions()
        {
            // Arrange
            var stream = new TestableInteractionStream();
            var usersClient = new UsersClient(_microsoftClientFixture.Client, _usersClientlogger);

            var timeRange = new TimeRange(new DateTime(2023, 04, 10), new DateTime(2023, 04, 11));
            var syncContext = new SyncContext(Guid.NewGuid(), ConnectorConfig.Empty, new ConnectorProperties(), new SecureString(), timeRange, Mock.Of<IStatusLogger>(), Mock.Of<IHashingService>());
            var users = await usersClient.GetUsersAsync(syncContext);
            var employees = EmployeesMapper.ToEmployees(users, HashFunction.Empty, EmployeeFilter.Empty, true);

            var interactionFactory = new MeetingInteractionFactory(HashFunction.Empty, employees, NullLogger<MeetingInteractionFactory>.Instance);
            var calednarClient = new CalendarClient(_microsoftClientFixture.Client, Mock.Of<ITasksStatusesCache>(), _calendarClientlogger);

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
}