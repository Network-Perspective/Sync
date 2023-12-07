using System;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Interactions
{
    public class InteractionTests
    {
        [Fact]
        public void ShouldCreateStableId()
        {
            // Arrange
            var source = Employee.CreateExternal("email_1@networkperspective.io");
            var target = Employee.CreateExternal("email_2@networkperspective.io");
            var timestamp = new DateTime(2022, 01, 01);

            // Act
            var interaction = Interaction.CreateEmail(timestamp, source, target, "foo");

            // Assert
            interaction.Id.Should().Be("c68945717f7ccf9c20168a93dd489e31");
        }

        [Fact]
        public void ShouldNotChangeIdDuringHash()
        {
            // Arrange
            var source = Employee.CreateExternal("email_1@networkperspective.io");
            var target = Employee.CreateExternal("email_2@networkperspective.io");
            var timestamp = new DateTime(2022, 01, 01);
            var interaction = Interaction.CreateEmail(timestamp, source, target, "foo");

            // Act
            var hashedInteraction = interaction.Hash(x => $"{x}_hashed");

            // Assert
            interaction.Id.Should().Be(hashedInteraction.Id);
        }
    }
}