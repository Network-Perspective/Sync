
using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Networks.Filters;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Networks.Filters
{
    public class BlacklistTests
    {
        [Fact]
        public void ShouldForbidOnEmailInBlacklist()
        {
            // Arrange
            var blackList = new Blacklist(new[] { "email1@foo.bar", "email2@foo.bar" });

            // Act
            var result = blackList.IsForbidden("email1@foo.bar");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotForbidOnEmailNotInBlacklist()
        {
            // Arrange
            var blackList = new Blacklist(new[] { "email1@foo.bar", "email2@foo.bar" });

            // Act
            var result = blackList.IsForbidden("john.doe@networkperspective.io");

            // Assert
            result.Should().BeFalse();
        }

    }
}