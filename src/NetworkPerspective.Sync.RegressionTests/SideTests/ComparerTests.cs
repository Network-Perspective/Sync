using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace NetworkPerspective.Sync.RegressionTests.SideTests
{
    public class ComparerTests
    {
        [Fact]
        public void ShouldProduceCorrectResult()
        {
            // Arrange
            var list1 = new[] { 1, 2, 3 };
            var list2 = new[] { 2, 3, 4, 5 };
            var comparer = new Services.Comparer<int>(EqualityComparer<int>.Default);


            // Act
            var result = comparer.Compare(list1, list2);

            // Assert
            result.InBoth.Should().BeEquivalentTo(new[] { 2, 3 });
            result.OnlyInOld.Should().BeEquivalentTo(new[] { 1 });
            result.OnlyInNew.Should().BeEquivalentTo(new[] { 4, 5 });
        }
    }
}