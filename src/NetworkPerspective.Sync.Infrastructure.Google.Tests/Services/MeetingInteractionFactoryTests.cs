using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Google.Apis.Calendar.v3.Data;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Services
{
    public class MeetingInteractionFactoryTests
    {
        [Fact]
        public void ShouldCreateFromMeeting()
        {
            // Arrange
            const string user1Email = "user1@networkperspective.io";
            const string user2Email = "<user2@networkperspective.io> user2";
            const string user3Email = "user3@networkperspective.io";
            const string user4Email = "user4@networkperspective.io";
            const string user5Email = "user5@networkperspective.io";

            var meeting = new Event()
            {
                Attendees = new[]
                {
                    new EventAttendee { Email = user1Email },
                    new EventAttendee { Email = user2Email },
                    new EventAttendee { Email = user3Email }
                },
                Recurrence = null,
                Start = new EventDateTime { DateTime = new DateTime(2020, 01, 01, 10, 00, 0) },
                End = new EventDateTime { DateTime = new DateTime(2020, 01, 01, 11, 30, 0) }
            };

            var employees = new List<Employee>()
                .Add(user1Email)
                .Add("user2@networkperspective.io")
                .Add(user3Email)
                .Add(user4Email)
                .Add(user5Email);

            var employeesCollection = new EmployeeCollection(employees, null);

            // Act
            var interactions = new MeetingInteractionFactory(x => $"{x}_hashed", employeesCollection)
                .Create(meeting, null);

            // Assert
            var emailInteractions = interactions.Where(x => x.Type == InteractionType.Meetings);
            emailInteractions.Should().HaveCount(6);
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == "user2@networkperspective.io_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == "user2@networkperspective.io_hashed" && x.Target.Id.PrimaryId == $"{user1Email}_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == $"{user3Email}_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{user3Email}_hashed" && x.Target.Id.PrimaryId == $"{user1Email}_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == $"{user3Email}_hashed" && x.Target.Id.PrimaryId == "user2@networkperspective.io_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Duration == 90 && x.Source.Id.PrimaryId == "user2@networkperspective.io_hashed" && x.Target.Id.PrimaryId == $"{user3Email}_hashed").Should().ContainSingle();
        }

    }
}