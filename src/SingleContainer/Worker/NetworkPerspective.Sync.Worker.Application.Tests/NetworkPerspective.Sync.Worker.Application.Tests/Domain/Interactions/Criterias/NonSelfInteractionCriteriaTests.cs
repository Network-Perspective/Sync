﻿using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Interactions;
using NetworkPerspective.Sync.Worker.Application.Domain.Interactions.Criterias;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Domain.Interactions.Criterias
{
    public class NonSelfInteractionCriteriaTests
    {
        [Theory]
        [InlineData("foo", "foo")]
        [InlineData("foo", "Foo")]
        [InlineData("Foo", "foo")]
        public void ShouldFilterOutSelfInteractions(string sourceId, string targetId)
        {
            // Arrange
            var source = Employee.CreateInternal(EmployeeId.Create(sourceId, sourceId), Array.Empty<Group>());
            var target = Employee.CreateInternal(EmployeeId.Create(targetId, targetId), Array.Empty<Group>());
            var interactions = new[] { Interaction.CreateEmail(DateTime.UtcNow, source, target, "id1") };

            // Act
            var result = new NonSelfInteractionCriteria(NullLogger<NonSelfInteractionCriteria>.Instance)
                .MeetCriteria(interactions);

            // Asset
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("foo", "bar")]
        public void ShouldNotFilterOutCrossInteractions(string sourceId, string targetId)
        {
            // Arrange
            var source = Employee.CreateInternal(EmployeeId.Create(sourceId, sourceId), Array.Empty<Group>());
            var target = Employee.CreateInternal(EmployeeId.Create(targetId, targetId), Array.Empty<Group>());
            var interactions = new[] { Interaction.CreateEmail(DateTime.UtcNow, source, target, "id1") };

            // Act
            var result = new NonSelfInteractionCriteria(NullLogger<NonSelfInteractionCriteria>.Instance)
                .MeetCriteria(interactions);

            // Asset
            result.Should().BeEquivalentTo(interactions);
        }
    }
}