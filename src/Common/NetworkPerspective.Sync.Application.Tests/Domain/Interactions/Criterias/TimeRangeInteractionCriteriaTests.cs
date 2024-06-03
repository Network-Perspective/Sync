using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Interactions.Criterias;
using NetworkPerspective.Sync.Utils.Models;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Interactions.Criterias
{
    public class TimeRangeInteractionCriteriaTests
    {
        [Fact]
        public void ShouldFilterOutInteractionOutsideTimeRange()
        {
            // Arrange
            var sourceId = EmployeeId.Create("foo", "foo_id");
            var source = Employee.CreateInternal(sourceId, Array.Empty<Group>());
            var targetId = EmployeeId.Create("bar", "bar_id");
            var target = Employee.CreateInternal(targetId, Array.Empty<Group>());
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
            var sourceId = EmployeeId.Create("foo", "foo_id");
            var source = Employee.CreateInternal(sourceId, Array.Empty<Group>());
            var targetId = EmployeeId.Create("bar", "bar_id");
            var target = Employee.CreateInternal(targetId, Array.Empty<Group>());
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