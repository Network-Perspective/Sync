using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.RegressionTests.Interactions;

using Xunit;

namespace NetworkPerspective.Sync.RegressionTests.SideTests
{
    public class VertexIdEqualityComparerTests
    {
        public class GetHashCodeMethod : VertexIdEqualityComparerTests
        {
            [Fact]
            public void ShouldReturnTheSameCodeForTheSameDicts()
            {
                // Arrange
                var comparer = new VertexIdEqualityComparer();
                var dict1 = new Dictionary<string, string>
                {
                    { "Id1", "Value1" },
                    { "Id2", "Value2" }
                };

                var dict2 = new Dictionary<string, string>
                {
                    { "Id2", "Value2" },
                    { "Id1", "Value1" }
                };


                // Act
                var hashCode1 = comparer.GetHashCode(dict1);
                var hashCode2 = comparer.GetHashCode(dict2);

                // Assert
                hashCode1.Should().Be(hashCode2);
            }
        }

        public class EqualsMethod : VertexIdEqualityComparerTests
        {
            [Fact]
            public void ShouldReturnTrueForEquals()
            {
                // Arrange
                var comparer = new VertexIdEqualityComparer();
                var dict1 = new Dictionary<string, string>
                {
                    { "Id1", "Value1" },
                    { "Id2", "Value2" }
                };

                var dict2 = new Dictionary<string, string>
                {
                    { "Id2", "Value2" },
                    { "Id1", "Value1" }
                };


                // Act
                var result = comparer.Equals(dict1, dict2);

                // Assert
                result.Should().BeTrue();
            }
        }
    }
}
