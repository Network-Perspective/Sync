using System;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Networks.Filters;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Networks.Filters
{
    public class WhitelistTests
    {
        [Theory]
        [InlineData("john.doe@networkperspective.io", "*")]
        [InlineData("john.doe@networkperspective.io", "*@*")]
        [InlineData("john.doe@networkperspective.io", "*@networkperspective.io")]
        [InlineData("john.doe@networkperspective.io", "john.doe*")]
        [InlineData("john.doe@networkperspective.io", "john.doe?networkperspective.io")]
        [InlineData("john.doe@networkperspective.io", "john.doe@networkperspective.io")]
        public void ShouldReturnTrueOnMatch(string email, string allowedDomain)
        {
            // Arrange
            var domainWhitelist = new Whitelist(new[] { allowedDomain });

            // Act
            var result = domainWhitelist.IsAllowed(email);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalseForNotAllowedEmail()
        {
            // Arrange
            var whitelist = new Whitelist(new[] { "networkperspective.io", "?networkperspective.com", "mapaorganizacji.com" });

            // Act
            var isWhiteListed = whitelist.IsAllowed("john.doe@networkperspective.com");

            // Assert
            isWhiteListed.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalseForEmptyWhiteList()
        {
            // Arrange
            var whitelist = new Whitelist(Array.Empty<string>());

            // Act
            var isWhiteListed = whitelist.IsAllowed("foo");

            // Assert
            isWhiteListed.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalseOnNullInput()
        {
            // Arrange
            var whitelist = new Whitelist(new[] { "*" });

            // Act
            var isWhiteListed = whitelist.IsAllowed(null);

            // Assert
            isWhiteListed.Should().BeFalse();
        }
    }
}