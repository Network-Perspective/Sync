using System;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Aggregation;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Aggregation
{
    public class ActionsAggregatorTests
    {
        [Fact]
        public void ShouldCollectMessagesCount()
        {
            // Arrange
            var day1 = new DateTime(2000, 01, 01);
            var day2 = new DateTime(2000, 01, 02);
            var day3 = new DateTime(2000, 01, 03);

            var aggregator = new ActionsAggregator("foo");

            aggregator.Add(day1);
            aggregator.Add(day2);
            aggregator.Add(day2);
            aggregator.Add(day3.AddMinutes(10));
            aggregator.Add(day3.AddHours(3));
            aggregator.Add(day3);

            // Act
            var result = aggregator.GetActionsPerDay();

            // Assert
            result.Count.Should().Be(3);

            result[day1].Should().Be(1);
            result[day2].Should().Be(2);
            result[day3].Should().Be(3);
        }
    }
}