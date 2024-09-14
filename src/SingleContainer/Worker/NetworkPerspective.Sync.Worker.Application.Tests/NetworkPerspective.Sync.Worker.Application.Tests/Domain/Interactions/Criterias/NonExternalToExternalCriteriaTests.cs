using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Interactions;
using NetworkPerspective.Sync.Worker.Application.Domain.Interactions.Criterias;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Domain.Interactions.Criterias
{
    public class NonExternalToExternalCriteriaTests
    {
        private static readonly Employee Internal1 = Employee.CreateInternal(EmployeeId.Create("internal1", "internal1_id"), Array.Empty<Group>());
        private static readonly Employee Internal2 = Employee.CreateInternal(EmployeeId.Create("internal2", "internal2_id"), Array.Empty<Group>());
        private static readonly Employee External1 = Employee.CreateExternal("external1");
        private static readonly Employee External2 = Employee.CreateExternal("external2");

        [Fact]
        public void ShouldFilterOutExternalToExternalInteraction()
        {
            // Arrange
            var externalToExternal = Interaction.CreateEmail(DateTime.UtcNow, External1, External2, "id1");
            var externalToInternal = Interaction.CreateEmail(DateTime.UtcNow, External1, Internal1, "id2");
            var internalToExternal = Interaction.CreateEmail(DateTime.UtcNow, Internal1, External1, "id3");
            var internalToInternal = Interaction.CreateEmail(DateTime.UtcNow, Internal1, Internal2, "id4");
            var interactions = new[] { externalToExternal, externalToInternal, internalToExternal, internalToInternal };

            // Act
            var result = new NonExternalToExternalCriteria(NullLogger<NonExternalToExternalCriteria>.Instance)
                .MeetCriteria(interactions);

            // Assert
            result.Should().BeEquivalentTo(new[] { externalToInternal, internalToExternal, internalToInternal });
        }
    }
}