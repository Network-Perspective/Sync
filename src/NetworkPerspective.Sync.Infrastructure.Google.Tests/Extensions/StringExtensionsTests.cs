using System.Linq;

using FluentAssertions;

using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("John Doe <john.doe@networkperspective.io>", "john.doe@networkperspective.io")]
        [InlineData("john.doe@networkperspective.io", "john.doe@networkperspective.io")]
        public void ShouldExtractEmail(string input, string expectedOutput)
        {
            // Arrange
            var array = new[] { input };

            // Act
            var result = array.ExtractEmailAddress();

            // Assert
            result.Single().Should().Be(expectedOutput);
        }
    }
}
