using System;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Meetings;
using NetworkPerspective.Sync.Application.Exceptions;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class ReocurrenceFactoryTests
    {
        [Theory]
        [InlineData("RRULE:FREQ=MONTHLY;INTERVAL=2", RecurrenceType.Monthly, 2)]
        [InlineData("RRULE:FREQ=YEARLY", RecurrenceType.Yearly, 1)]
        [InlineData("RRULE:FREQ=MONTHLY", RecurrenceType.Monthly, 1)]
        [InlineData("RRULE:FREQ=WEEKLY", RecurrenceType.Weekly, 1)]
        [InlineData("RRULE:FREQ=DAILY", RecurrenceType.Daily, 1)]
        [InlineData("RRULE:FREQ=HOURLY", RecurrenceType.Hourly, 1)]
        [InlineData("RRULE:FREQ=MINUTELY", RecurrenceType.Minutely, 1)]
        [InlineData("RRULE:FREQ=SECONDLY", RecurrenceType.Secondly, 1)]
        [InlineData("RRULE:FREQ=WEEKLY;BYDAY=FR", RecurrenceType.Weekly, 1)]
        [InlineData("RRULE:FREQ=MONTHLY;UNTIL=20230204T233000Z;INTERVAL=1;BYDAY=1SA", RecurrenceType.Monthly, 1)]
        public void ShouldCreateFromRfc5545(string input, RecurrenceType expectedType, int expectedInterval)
        {
            // Act
            var result = new RecurrenceFactory().CreateFromRRule(input);

            // Assert
            result.Type.Should().Be(expectedType);
            result.Interval.Should().Be(expectedInterval);
        }

        [Fact]
        public void ShouldThrowUnexpectedRecurrenceFormatExceptionOnInvalidFormat()
        {
            // Arrange
            var malformedRecurrenceString = "RRULE:FRE=YEARLY";
            Action action = () => new RecurrenceFactory().CreateFromRRule(malformedRecurrenceString);

            // Act
            var exception = Record.Exception(action);

            // Assert
            exception.Should().BeOfType<UnexpectedRecurrenceFormatException>();
            exception.Message.Should().Contain(malformedRecurrenceString);
        }
    }
}