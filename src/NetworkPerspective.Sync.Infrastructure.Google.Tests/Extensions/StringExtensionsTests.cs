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
        [InlineData("\"Giant; \\\"Big\\\" Box\" <sysservices@example.net>", "sysservices@example.net")]
        [InlineData("\"Joe Q. Public\" <john.q.public@example.com>", "john.q.public@example.com")]
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