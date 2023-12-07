using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Combinations;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Combinations
{
    public class CombinationFactoryTests
    {
        [Fact]
        public void ShouldGenerateCorrectCombinations()
        {
            // Arrange
            var input = new[] { "foo", "bar", "baz" };
            var expectedResult = new[]
            {
                new Combination<string>("foo", "bar"),
                new Combination<string>("foo", "baz"),
                new Combination<string>("bar", "foo"),
                new Combination<string>("bar", "baz"),
                new Combination<string>("baz", "foo"),
                new Combination<string>("baz", "bar")
            };

            // Act
            var result = CombinationFactory<string>.CreateCombinations(input);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(2, 2)]
        [InlineData(3, 6)]
        [InlineData(4, 12)]
        public void ShouldGenerateCorrectCombinationsCount(int inputCount, int expectedCombnationsCount)
        {
            // Arrange
            var input = new List<string>();
            for (int i = 0; i < inputCount; i++)
                input.Add(i.ToString());

            // Act
            var result = CombinationFactory<string>.CreateCombinations(input);

            // Assert
            result.Should().HaveCount(expectedCombnationsCount);
        }
    }
}