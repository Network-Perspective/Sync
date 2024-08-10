using System.Linq;

using FluentAssertions;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSource.Google.Tests.Extensions
{
    public class StringExtensionsTests
    {
        public class ExtractEmailAddress : StringExtensionsTests
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

        public class GetUserEmails : StringExtensionsTests
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("John Doe <john.doe@networkperspective.io>", "john.doe@networkperspective.io")]
            [InlineData("John Doe <john.doe@networkperspective.io>, \"Giant;, \\\"Big\\\" Box\" <sysservices@example.net>", "john.doe@networkperspective.io", "sysservices@example.net")]
            [InlineData("Group:john.doe@networkperspective.io, sysservices@example.net; foo@bar.com", "john.doe@networkperspective.io", "sysservices@example.net", "foo@bar.com")]
            [InlineData("Group:; foo@bar.com", "foo@bar.com")]
            public void ShouldExtractEmail(string input, params string[] expectedEmails)
            {
                // Act
                var result = input.GetUserEmails();

                // Assert
                result.Should().BeEquivalentTo(expectedEmails);
            }
        }
    }
}