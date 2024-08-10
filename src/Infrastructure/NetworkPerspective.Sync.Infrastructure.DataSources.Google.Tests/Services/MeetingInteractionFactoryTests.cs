using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Google.Apis.Calendar.v3.Data;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSource.Google.Tests.Services
{
    public class MeetingInteractionFactoryTests
    {
        [Fact]
        public void ShouldCreateAllForSmallMeeting()
        {
            // Arrange
            const string user1Email = "user1@networkperspective.io";
            const string user2Email = "user2 <user2@networkperspective.io>";
            const string user3Email = "user3@networkperspective.io";
            const string user3Email_alias = "user3_alias@networkperspective.io";
            const string externalUserEmail = "external@foo.com";

            var user3Id = EmployeeId.CreateWithAliases(user3Email, "test", new[] { user3Email_alias }, EmployeeFilter.Empty);
            var user3 = Employee.CreateInternal(user3Id, Enumerable.Empty<Group>());

            var meeting = new Event()
            {
                Attendees = new[]
                {
                    new EventAttendee { Email = user1Email },
                    new EventAttendee { Email = user2Email },
                    new EventAttendee { Email = user3Email },
                    new EventAttendee { Email = user3Email_alias },
                    new EventAttendee { Email = externalUserEmail }
                },
                Recurrence = null,
                Start = new EventDateTime { DateTimeDateTimeOffset = new DateTime(2020, 01, 01, 10, 00, 0) },
                End = new EventDateTime { DateTimeDateTimeOffset = new DateTime(2020, 01, 01, 11, 30, 0) }
            };

            var employees = new List<Employee> { user3 }
                .Add(user1Email)
                .Add("user2@networkperspective.io");

            var employeesCollection = new EmployeeCollection(employees, null);

            // Act
            var interactions = new MeetingInteractionFactory(x => $"{x}_hashed", employeesCollection, NullLogger<MeetingInteractionFactory>.Instance)
                .CreateForUser(meeting, user1Email, null);

            // Assert
            var emailInteractions = interactions.Where(x => x.Type == InteractionType.Meetings);
            emailInteractions.Should().HaveCount(4);
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == "user2@networkperspective.io_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == $"{user3Email}_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == $"{externalUserEmail}_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{externalUserEmail}_hashed" && x.Target.Id.PrimaryId == $"{user1Email}_hashed").Should().ContainSingle();
        }

        [Fact]
        public void ShouldCreateAllForBigMeeting()
        {
            // Arrange
            const string user1Email = "user1@networkperspective.io";
            const string externalUserEmail = "external@foo.com";

            var emails = Enumerable.Range(1, 110)
                .Select(x => $"user_{x}@networkperspective.io");

            var attendees = new[] { new EventAttendee { Email = externalUserEmail } };

            var meeting = new Event()
            {
                Attendees = attendees.Union(emails.Select(x => new EventAttendee { Email = x })).ToArray(),
                Recurrence = null,
                Start = new EventDateTime { DateTimeDateTimeOffset = new DateTime(2020, 01, 01, 10, 00, 0) },
                End = new EventDateTime { DateTimeDateTimeOffset = new DateTime(2020, 01, 01, 11, 30, 0) }
            };

            var employees = new List<Employee>();

            foreach (var email in emails)
                employees = employees.Add(email);

            var employeesCollection = new EmployeeCollection(employees, null);

            // Act
            var interactions = new MeetingInteractionFactory(x => $"{x}_hashed", employeesCollection, NullLogger<MeetingInteractionFactory>.Instance)
                .CreateForUser(meeting, user1Email, null);

            // Assert
            var emailInteractions = interactions.Where(x => x.Type == InteractionType.Meetings);
            emailInteractions.Should().HaveCount(1);
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == $"{externalUserEmail}_hashed").Should().ContainSingle();
        }

    }
}