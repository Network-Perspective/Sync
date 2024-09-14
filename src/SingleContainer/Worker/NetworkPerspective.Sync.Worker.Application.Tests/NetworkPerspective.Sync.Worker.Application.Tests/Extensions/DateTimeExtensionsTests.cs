using System;

using FluentAssertions;

using NetworkPerspective.Sync.Worker.Application.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Extensions
{
    public class DateTimeExtensionsTests
    {
        [Theory]
        [InlineData("2020-01-01T10:22:11.000", "00:10:00", "2020-01-01T10:20:00.000")]
        [InlineData("2020-01-01T10:22:11.000", "00:05:00", "2020-01-01T10:20:00.000")]
        [InlineData("2020-01-01T10:27:11.000", "00:05:00", "2020-01-01T10:25:00.000")]
        [InlineData("2020-01-01T10:22:11.000", "00:01:00", "2020-01-01T10:22:00.000")]
        public void Should(string input, string rounding, string expected)
        {
            // Arrange
            var inputDt = DateTime.Parse(input);
            var rountingTs = TimeSpan.Parse(rounding);

            // Act
            var result = inputDt.Bucket(rountingTs);

            // Assert
            result.Should().Be(DateTime.Parse(expected));
        }
    }
}