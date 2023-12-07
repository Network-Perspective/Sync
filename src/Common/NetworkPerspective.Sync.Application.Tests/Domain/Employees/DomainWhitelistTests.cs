using System;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Employees
{
    public class DomainWhitelistTests
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
            var domainWhitelist = new DomainWhitelist(new[] { allowedDomain });

            // Act
            var result = domainWhitelist.IsInAllowedDomain(email);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalseForNotAllowedEmail()
        {
            // Arrange
            var whitelist = new DomainWhitelist(new[] { "networkperspective.io", "?networkperspective.com", "mapaorganizacji.com" });

            // Act
            var isWhiteListed = whitelist.IsInAllowedDomain("john.doe@networkperspective.com");

            // Assert
            isWhiteListed.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrueForEmptyWhiteList()
        {
            // Arrange
            var whitelist = new DomainWhitelist(Array.Empty<string>());

            // Act
            var isWhiteListed = whitelist.IsInAllowedDomain("foo");

            // Assert
            isWhiteListed.Should().BeTrue();
        }
    }
}