using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Interactions.Criterias;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Interactions.Criterias
{
    public class NonBotInteractionCriteriaTests
    {
        [Fact]
        public void ShouldFilterOutBotTargetInteraction()
        {
            // Arrange
            var interactions = new[]
            {
                Interaction.CreateEmail(DateTime.UtcNow, Employee.CreateBot("bot1"), Employee.CreateInternal(EmployeeId.Create("internal1", "internal1_id"), Array.Empty<Group>()), "id1"),
                Interaction.CreateEmail(DateTime.UtcNow, Employee.CreateInternal(EmployeeId.Create("internal2", "internal2_id"), Array.Empty<Group>()), Employee.CreateBot("bot2"), "id2"),
                Interaction.CreateEmail(DateTime.UtcNow, Employee.CreateBot("bot3"), Employee.CreateBot("internal4"), "id1")
            };

            // Act
            var result = new NonBotInteractionCriteria(NullLogger<NonBotInteractionCriteria>.Instance)
                .MeetCriteria(interactions);

            // Asset
            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldNotFilterOutUsersInteraction()
        {
            // Arrange
            var source = Employee.CreateInternal(EmployeeId.Create("foo", "foo_id"), Array.Empty<Group>());
            var target = Employee.CreateExternal("bar");
            var interactions = new[] { Interaction.CreateEmail(DateTime.UtcNow, source, target, "id1") };

            // Act
            var result = new NonBotInteractionCriteria(NullLogger<NonBotInteractionCriteria>.Instance)
                .MeetCriteria(interactions);

            // Asset
            result.Should().BeEquivalentTo(interactions);
        }
    }
}