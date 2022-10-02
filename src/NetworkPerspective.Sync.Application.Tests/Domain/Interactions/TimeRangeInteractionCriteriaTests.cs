using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Interactions
{
    public class TimeRangeInteractionCriteriaTests
    {
        [Fact]
        public void ShouldFilterOutInteractionOutsideTimeRange()
        {
            // Arrange
            var source = Employee.CreateInternal("foo", "foo_id", Array.Empty<Group>());
            var target = Employee.CreateInternal("bar", "foo_id", Array.Empty<Group>());
            var interactions = new[] { Interaction.CreateEmail(new DateTime(2020, 01, 03), source, target, "id1") };

            var timeRange = new TimeRange(
                new DateTime(2020, 01, 01, 01, 01, 01),
                new DateTime(2020, 01, 02, 01, 01, 01));

            // Act
            var result = new TimeRangeInteractionCriteria(timeRange, NullLogger<TimeRangeInteractionCriteria>.Instance)
                .MeetCriteria(interactions);

            // Asset
            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldNotFilterOutInteractionWithinTimeRange()
        {
            // Arrange
            var source = Employee.CreateInternal("foo", "foo_id", Array.Empty<Group>());
            var target = Employee.CreateInternal("bar", "bar_id", Array.Empty<Group>());
            var interactions = new[] { Interaction.CreateEmail(new DateTime(2020, 01, 01, 10, 10, 10), source, target, "id1") };

            var timeRange = new TimeRange(
                new DateTime(2020, 01, 01, 01, 01, 01),
                new DateTime(2020, 01, 02, 01, 01, 01));

            // Act
            var result = new TimeRangeInteractionCriteria(timeRange, NullLogger<TimeRangeInteractionCriteria>.Instance)
                .MeetCriteria(interactions);

            // Asset
            result.Should().BeEquivalentTo(interactions);
        }
    }
}