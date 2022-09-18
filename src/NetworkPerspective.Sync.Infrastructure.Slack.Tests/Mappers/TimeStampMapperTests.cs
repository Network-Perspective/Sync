using System;
using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Infrastructure.Slack.Mappers;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Mappers
{
    public class TimeStampMapperTests
    {

        public static IEnumerable<object[]> SlackTimeStampAndDateTime()
        {
            yield return new object[] { "1512104434.000490", new DateTime(636477012340004900) };
            yield return new object[] { "1623456323.432112", new DateTime(637590531234321120) };
        }

        [Theory]
        [MemberData(nameof(SlackTimeStampAndDateTime))]
        public void ShouldReturnExpectedDateTime(string slackTimeStamp, DateTime dateTime)
        {
            // Act
            var result = TimeStampMapper.SlackTimeStampToDateTime(slackTimeStamp);

            // Assert
            result.Should().Be(dateTime);
        }

        [Theory]
        [MemberData(nameof(SlackTimeStampAndDateTime))]
        public void ShouldReturnExpectedSlackTimeStamp(string slackTimeStamp, DateTime dateTime)
        {
            // Act
            var result = TimeStampMapper.DateTimeToSlackTimeStamp(dateTime);

            // Assert
            result.Should().Be(slackTimeStamp);
        }
    }
}