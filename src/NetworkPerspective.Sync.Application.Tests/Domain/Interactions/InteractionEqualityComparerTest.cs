﻿using System;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Interactions
{
    public class InteractionEqualityComparerTest
    {
        [Fact]
        public void ShoudReturnFalseOnNotEqualDateTime()
        {
            // Arrange
            var timestamp1 = DateTime.UtcNow;
            var timestamp2 = timestamp1.AddHours(1);
            var source = Employee.CreateInternal("source", "souce_1", string.Empty, Array.Empty<Group>());
            var target = Employee.CreateInternal("target", "target_1", string.Empty, Array.Empty<Group>());
            var eventId = "eventId";
            var parentEventId = "parentEventId";
            var channelId = "channelId";

            var interaction1 = Interaction.CreateChatReaction(timestamp1, source, target, eventId, parentEventId, channelId);
            var interaction2 = Interaction.CreateChatReaction(timestamp2, source, target, eventId, parentEventId, channelId);

            // Act
            var result = new InteractionEqualityComparer().Equals(interaction1, interaction2);
            var hash1 = new InteractionEqualityComparer().GetHashCode(interaction1);
            var hash2 = new InteractionEqualityComparer().GetHashCode(interaction2);

            // Assert
            result.Should().BeFalse();
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void ShoudReturnFalseOnNotEqualSource()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var source1 = Employee.CreateInternal("source1", "source1_id", string.Empty, Array.Empty<Group>());
            var source2 = Employee.CreateInternal("source2", "source2_id", string.Empty, Array.Empty<Group>());
            var target = Employee.CreateInternal("target", "target_id", string.Empty, Array.Empty<Group>());
            var eventId = "eventId";
            var parentEventId = "parentEventId";
            var channelId = "channelId";

            var interaction1 = Interaction.CreateChatReaction(timestamp, source1, target, eventId, parentEventId, channelId);
            var interaction2 = Interaction.CreateChatReaction(timestamp, source2, target, eventId, parentEventId, channelId);

            // Act
            var result = new InteractionEqualityComparer().Equals(interaction1, interaction2);
            var hash1 = new InteractionEqualityComparer().GetHashCode(interaction1);
            var hash2 = new InteractionEqualityComparer().GetHashCode(interaction2);

            // Assert
            result.Should().BeFalse();
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void ShoudReturnFalseOnNotEqualTarget()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var source = Employee.CreateInternal("source", "source_id", string.Empty, Array.Empty<Group>());
            var target1 = Employee.CreateInternal("target1", "target1_id", string.Empty, Array.Empty<Group>());
            var target2 = Employee.CreateInternal("target2", "target2_id", string.Empty, Array.Empty<Group>());
            var eventId = "eventId";
            var parentEventId = "parentEventId";
            var channelId = "channelId";

            var interaction1 = Interaction.CreateChatReaction(timestamp, source, target1, eventId, parentEventId, channelId);
            var interaction2 = Interaction.CreateChatReaction(timestamp, source, target2, eventId, parentEventId, channelId);

            // Act
            var result = new InteractionEqualityComparer().Equals(interaction1, interaction2);
            var hash1 = new InteractionEqualityComparer().GetHashCode(interaction1);
            var hash2 = new InteractionEqualityComparer().GetHashCode(interaction2);

            // Assert
            result.Should().BeFalse();
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void ShoudReturnFalseOnNotEqualActionType()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var source = Employee.CreateInternal("source", "source_id", string.Empty, Array.Empty<Group>());
            var target = Employee.CreateInternal("target", "targer_id", string.Empty, Array.Empty<Group>());
            var eventId = "eventId";
            var parentEventId = "parentEventId";
            var channelId = "channelId";

            var interaction1 = Interaction.CreateChatReply(timestamp, source, target, eventId, parentEventId, channelId);
            var interaction2 = Interaction.CreateChatReaction(timestamp, source, target, eventId, parentEventId, channelId);

            // Act
            var result = new InteractionEqualityComparer().Equals(interaction1, interaction2);
            var hash1 = new InteractionEqualityComparer().GetHashCode(interaction1);
            var hash2 = new InteractionEqualityComparer().GetHashCode(interaction2);

            // Assert
            result.Should().BeFalse();
            hash1.Should().NotBe(hash2);
        }
    }
}