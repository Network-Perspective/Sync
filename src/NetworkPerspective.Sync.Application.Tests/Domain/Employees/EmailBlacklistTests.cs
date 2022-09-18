
using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Employees
{
    public class EmailBlacklistTests
    {
        [Fact]
        public void ShouldForbidOnEmailInBlacklist()
        {
            // Arrange
            var blackList = new EmailBlacklist(new[] { "email1@foo.bar", "email2@foo.bar" });

            // Act
            var result = blackList.IsForbiddenEmail("email1@foo.bar");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotForbidOnEmailNotInBlacklist()
        {
            // Arrange
            var blackList = new EmailBlacklist(new[] { "email1@foo.bar", "email2@foo.bar" });

            // Act
            var result = blackList.IsForbiddenEmail("john.doe@networkperspective.io");

            // Assert
            result.Should().BeFalse();
        }

    }
}