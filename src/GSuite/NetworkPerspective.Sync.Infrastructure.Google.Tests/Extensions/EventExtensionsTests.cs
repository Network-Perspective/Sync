using System;

using FluentAssertions;

using Google.Apis.Calendar.v3.Data;

using NetworkPerspective.Sync.Application.Domain.Meetings;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Extensions
{
    public class EventExtensionsTests
    {
        public class GetDurationInMinutes
        {
            [Theory]
            [InlineData("2000-01-01T10:00", "2000-01-01T11:00", 60)]
            [InlineData(null, "2000-01-01T10:00", 0)]
            [InlineData("2000-01-01T10:00", null, 0)]
            [InlineData("2000-01-01T10:00", "2000-01-01T10:00", 0)]
            public void ShouldFindCorrectDuration(string start, string end, int expectedDuration)
            {
                // Arrange
                var startDateTime = start is null ? (DateTime?)null : DateTime.Parse(start);
                var endDateTime = end is null ? (DateTime?)null : DateTime.Parse(end);

                var @event = new Event
                {
                    Start = new EventDateTime { DateTime = startDateTime },
                    End = new EventDateTime { DateTime = endDateTime }
                };

                // Act
                var duration = @event.GetDurationInMinutes();

                // Assert
                duration.Should().Be(expectedDuration);
            }

            [Fact]
            public void ShouldNotThrowOnNull()
            {
                // Arrange
                var @event = new Event
                {
                    Start = null,
                    End = null
                };

                // Act
                var result = @event.GetDurationInMinutes();

                // Assert
                result.Should().Be(0);
            }
        }

        public class GetStart
        {
            [Theory]
            [InlineData("2000-01-01T10:00")]
            [InlineData("2010-01-11T10:00")]
            public void ShouldFindCorrectStart(string start)
            {
                // Arrange
                var startDateTime = DateTime.Parse(start);

                var @event = new Event
                {
                    Start = new EventDateTime { DateTime = startDateTime }
                };

                // Act
                var duration = @event.GetStart();

                // Assert
                duration.Should().Be(startDateTime);
            }

            [Fact]
            public void ShouldNotThrowOnNull()
            {
                // Arrange
                var @event1 = new Event
                {
                    Start = new EventDateTime { DateTime = null }
                };

                var @event2 = new Event
                {
                    Start = null
                };

                // Act Assert
                _ = @event1.GetStart();
                _ = @event2.GetStart();
            }
        }

        public class GetParticipants
        {
            [Fact]
            public void ShouldFindParticipants()
            {
                // Arrange
                var @event = new Event
                {
                    Attendees = new[]
                    {
                        new EventAttendee { Email = "John Doe <john.doe@networkperspective.io>"},
                        new EventAttendee { Email = "john.doe@worksmartona.com"}
                    }
                };

                // Act
                var participants = @event.GetParticipants();

                // Assert
                participants.Should().BeEquivalentTo(new[]
                {
                    "john.doe@networkperspective.io",
                    "john.doe@worksmartona.com"
                });
            }

            [Fact]
            public void ShouldReturnEmptyListOnNullAttendees()
            {
                // Arrange
                var @event1 = new Event
                {
                    Attendees = null
                };


                // Act
                var participants = @event1.GetParticipants();

                // Assert
                participants.Should().BeEmpty();
            }
        }

        public class GetRecurrence
        {
            [Fact]
            public void ShouldGetRecurrence()
            {
                // Arrange
                var @event = new Event
                {
                    Recurrence = new[]
                    {
                        "RRULE:FREQ=DAILY",
                        "EXDATE;TZID=Europe/Warsaw:20210923T113000,20210930T113000,20211223T113000,20220512T113000"
                    }
                };

                // Act
                var result = @event.GetRecurrence();

                // Assert
                result.Should().Be(RecurrenceType.Daily);
            }

            [Fact]
            public void ShouldGetNullOnNullRecurrence()
            {
                // Arrange
                var @event = new Event
                {
                    Recurrence = null
                };

                // Act
                var result = @event.GetRecurrence();

                // Assert
                result.Should().BeNull();
            }
        }

    }
}