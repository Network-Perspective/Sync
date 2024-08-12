using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Services
{
    public class CalendarClientTests : IClassFixture<GoogleClientFixture>
    {
        private readonly GoogleClientFixture _googleClientFixture;

        public CalendarClientTests(GoogleClientFixture googleClientFixture)
        {
            _googleClientFixture = googleClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldReturnInteractionsBasedOnGoogleCalendar()
        {
            // Arrange
            var email1 = "nptestuser12@worksmartona.com";
            var email2 = "john@worksmartona.com";
            var externalEmail = "maciej@networkperspective.io";
            var googleConfig = new GoogleConfig
            {
                ApplicationName = "gmail_app",
            };

            var client = new CalendarClient(Mock.Of<ITasksStatusesCache>(), Options.Create(googleConfig), new RetryPolicyProvider(NullLogger<RetryPolicyProvider>.Instance), _googleClientFixture.CredentialProvider, NullLogger<CalendarClient>.Instance);
            var timeRange = new TimeRange(new DateTime(2022, 12, 21), new DateTime(2022, 12, 22));
            var syncContext = new SyncContext(Guid.NewGuid(), string.Empty, ConnectorConfig.Empty, [], new SecureString(), timeRange, Mock.Of<IHashingService>());

            var employees = new List<Employee>()
                .Add(email1)
                .Add(email2);

            var employeesCollection = new EmployeeCollection(employees, null);

            var interactionFactory = new MeetingInteractionFactory((x) => $"{x}_hashed", employeesCollection, NullLogger<MeetingInteractionFactory>.Instance);

            var stream = new TestableInteractionStream();

            // Act
            await client.SyncInteractionsAsync(syncContext, stream, employeesCollection.GetAllInternal().Select(x => x.Id.PrimaryId), interactionFactory);

            // Assert
            stream.SentInteractions.Should().HaveCount(8);

            var interactions_1 = stream.SentInteractions.Where(x => x.Timestamp == new DateTime(2022, 12, 21, 08, 30, 00));
            interactions_1.Should().HaveCount(6);

            var interaction_1_1 = interactions_1.Single(x => x.Source.Id.PrimaryId == $"{email1}_hashed" && x.Target.Id.PrimaryId == $"{email2}_hashed");
            var interaction_1_2 = interactions_1.Single(x => x.Source.Id.PrimaryId == $"{email1}_hashed" && x.Target.Id.PrimaryId == $"{externalEmail}_hashed");
            var interaction_1_3 = interactions_1.Single(x => x.Source.Id.PrimaryId == $"{email2}_hashed" && x.Target.Id.PrimaryId == $"{email1}_hashed");
            var interaction_1_4 = interactions_1.Single(x => x.Source.Id.PrimaryId == $"{email2}_hashed" && x.Target.Id.PrimaryId == $"{externalEmail}_hashed");
            var interaction_1_5 = interactions_1.Single(x => x.Source.Id.PrimaryId == $"{externalEmail}_hashed" && x.Target.Id.PrimaryId == $"{email1}_hashed");
            var interaction_1_6 = interactions_1.Single(x => x.Source.Id.PrimaryId == $"{externalEmail}_hashed" && x.Target.Id.PrimaryId == $"{email2}_hashed");

            var interactions_2 = stream.SentInteractions.Where(x => x.Timestamp == new DateTime(2022, 12, 21, 14, 30, 00));
            interactions_2.Should().HaveCount(2);
            var interaction_2_1 = interactions_2.Single(x => x.Source.Id.PrimaryId == $"{email1}_hashed" && x.Target.Id.PrimaryId == $"{externalEmail}_hashed");
            var interaction_2_2 = interactions_2.Single(x => x.Source.Id.PrimaryId == $"{externalEmail}_hashed" && x.Target.Id.PrimaryId == $"{email1}_hashed");
        }
    }
}