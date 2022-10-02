using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class InteractionFilterTests
    {
        [Fact]
        public void ShouldFilter()
        {
            // Arrange
            var interactions = new[]
            {
                Interaction.CreateEmail(new DateTime(2020, 01, 01, 10, 10, 10 ), Employee.CreateInternal("user1", "user1_id", Array.Empty<Group>()), Employee.CreateInternal("user2", "user2_id", Array.Empty<Group>()), "id1"),
                Interaction.CreateEmail(new DateTime(2020, 01, 03), Employee.CreateInternal("user1", "user1_id", Array.Empty<Group>()), Employee.CreateInternal("user3", "user3_id", Array.Empty<Group>()), "id2"),
                Interaction.CreateEmail(new DateTime(2020, 01, 01, 10, 10, 10 ), Employee.CreateInternal("user1", "user1_id", Array.Empty<Group>()), Employee.CreateInternal("user1", "user1_id", Array.Empty<Group>()), "id3"),
                Interaction.CreateEmail(new DateTime(2020, 01, 01, 10, 10, 10 ), Employee.CreateInternal("user1", "user1_id", Array.Empty<Group>()), Employee.CreateBot("bot1"), "id4"),
                Interaction.CreateEmail(new DateTime(2020, 01, 01, 10, 10, 10 ), Employee.CreateInternal("user1", "user1_id", Array.Empty<Group>()), Employee.CreateExternal("user5"), "id5"),
                Interaction.CreateEmail(new DateTime(2020, 01, 01, 10, 10, 10 ), Employee.CreateExternal("external1"), Employee.CreateExternal("external2"), "id6"),
            };

            var expectedInteractions = new[]
            {
                Interaction.CreateEmail(new DateTime(2020, 01, 01, 10, 10, 10 ), Employee.CreateInternal("user1", "user1_id", Array.Empty<Group>()), Employee.CreateInternal("user2", "user2_id", Array.Empty<Group>()), "id1"),
                Interaction.CreateEmail(new DateTime(2020, 01, 01, 10, 10, 10 ), Employee.CreateInternal("user1", "user1_id", Array.Empty<Group>()), Employee.CreateExternal("user5"), "id5"),
            };

            var timeRange = new TimeRange(
                new DateTime(2020, 01, 01, 00, 00, 00),
                new DateTime(2020, 01, 02, 00, 00, 00));

            var filter = new InteractionsFilterFactory(NullLoggerFactory.Instance)
                .CreateInteractionsFilter(timeRange);

            // Act
            var filteredInteractions = filter.Filter(interactions);

            // Assert
            filteredInteractions.Should().BeEquivalentTo(expectedInteractions);
        }
    }
}