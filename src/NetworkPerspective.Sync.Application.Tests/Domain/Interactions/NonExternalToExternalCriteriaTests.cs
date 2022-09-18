using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Interactions
{
    public class NonExternalToExternalCriteriaTests
    {
        private static readonly Employee Internal1 = Employee.CreateInternal("internal1", "internal1_id", string.Empty, Array.Empty<Group>());
        private static readonly Employee Internal2 = Employee.CreateInternal("internal2", "internal2_id", string.Empty, Array.Empty<Group>());
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