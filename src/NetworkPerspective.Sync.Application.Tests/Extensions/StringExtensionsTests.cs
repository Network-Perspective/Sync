using System;

using FluentAssertions;
using FluentAssertions.Extensions;

using NetworkPerspective.Sync.Application.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Extensions
{
    public class StringExtensionsTests
    {
        public class GetMd5HashCode : StringExtensionsTests
        {
            [Theory]
            [InlineData("foo", "76fb6c8507d6cf995f6476646c9ea277")]
            [InlineData("bar", "768d574010cf7ff661b59773eabaeb47")]
            [InlineData("foobar", "91cce3e4110bcd1b7de17e50c6f889b8")]
            public void ShouldCreateNonRandomizedOutput(string input, string expectedOutput)
            {
                // Act
                var result = input.GetMd5HashCode();

                // Assert
                result.Should().Be(expectedOutput);
            }

            [Fact]
            public void ShouldBeFast()
            {
                // Act
                Action action = () =>
                {
                    for (var i = 0; i < 1000; i++)
                        "foo".GetMd5HashCode();
                };

                // Assert
                action.ExecutionTime().Should().BeLessThan(20.Milliseconds());
            }
        }
    }
}