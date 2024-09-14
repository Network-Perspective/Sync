using System;
using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;
using NetworkPerspective.Sync.RegressionTests.Interactions;

using Xunit;

namespace NetworkPerspective.Sync.RegressionTests.SideTests
{
    public class HashedInteractionEqualityComparerTests
    {
        public class GetHashCodeMethod : HashedInteractionEqualityComparerTests
        {
            [Fact]
            public void ShouldTheSameHashCodeFortheSameInteractions()
            {
                // Arrange
                var timestamp = DateTime.UtcNow;
                var eventId = "eventId";
                var label = new[] { HashedInteractionLabel.Email };
                var interaction1 = new HashedInteraction
                {
                    When = timestamp,
                    EventId = eventId,
                    Label = label,
                    SourceIds = new Dictionary<string, string>
                    {
                        { "Id1_1", "val1_1" },
                        { "Id2_1", "val2_1" }
                    },
                    TargetIds = new Dictionary<string, string>
                    {
                        { "Id1_2", "val1_2" },
                        { "Id2_2", "val2_2" }
                    },
                };
                var interaction2 = new HashedInteraction
                {
                    When = timestamp,
                    EventId = eventId,
                    Label = label,
                    SourceIds = new Dictionary<string, string>
                    {
                        { "Id1_1", "val1_1" },
                        { "Id2_1", "val2_1" }
                    },
                    TargetIds = new Dictionary<string, string>
                    {
                        { "Id1_2", "val1_2" },
                        { "Id2_2", "val2_2" }
                    }
                };

                var comparer = new HashedInteractionEqualityComparer();

                // Act
                var result1 = comparer.GetHashCode(interaction1);
                var result2 = comparer.GetHashCode(interaction2);

                // Assert
                result1.Should().Be(result2);
            }
        }

        public class EqualsMethod : HashedInteractionEqualityComparerTests
        {
            [Fact]
            public void ShouldReturnTrueOnEquals()
            {
                // Arrange
                var timestamp = DateTime.UtcNow;
                var eventId = "eventId";
                var label = new[] { HashedInteractionLabel.Email };
                var interaction1 = new HashedInteraction
                {
                    When = timestamp,
                    EventId = eventId,
                    Label = label,
                    SourceIds = new Dictionary<string, string>
                    {
                        { "Id1_1", "val1_1" },
                        { "Id2_1", "val2_1" }
                    },
                    TargetIds = new Dictionary<string, string>
                    {
                        { "Id1_2", "val1_2" },
                        { "Id2_2", "val2_2" }
                    },
                };
                var interaction2 = new HashedInteraction
                {
                    When = timestamp,
                    EventId = eventId,
                    Label = label,
                    SourceIds = new Dictionary<string, string>
                    {
                        { "Id1_1", "val1_1" },
                        { "Id2_1", "val2_1" }
                    },
                    TargetIds = new Dictionary<string, string>
                    {
                        { "Id1_2", "val1_2" },
                        { "Id2_2", "val2_2" }
                    }
                };

                var comparer = new HashedInteractionEqualityComparer();

                // Act
                var result = comparer.Equals(interaction1, interaction2);

                // Assert
                result.Should().BeTrue();
            }
        }
    }
}