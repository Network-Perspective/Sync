using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Meetings;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
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
                new Combination("foo", "bar"),
                new Combination("foo", "baz"),
                new Combination("bar", "foo"),
                new Combination("bar", "baz"),
                new Combination("baz", "foo"),
                new Combination("baz", "bar")
            };

            // Act
            var result = new CombinationFactory().CreateCombinations(input);

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
            var result = new CombinationFactory().CreateCombinations(input);

            // Assert
            result.Should().HaveCount(expectedCombnationsCount);
        }
    }
}