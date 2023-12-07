using System;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Networks;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Networks
{
    public class EmailFilterTests
    {
        [Fact]
        public void ShouldIndicateInternalIfIsAllowedAndNotForbidden()
        {
            // Arrange
            var email = "john.doe@networkperspective.io";
            var allowedDomains = new[] { "*networkperspective.io" };
            var forbiddenEmails = new[] { "alice.smith@networkperspective.io" };
            var filter = new EmailFilter(allowedDomains, forbiddenEmails);

            // Act
            var result = filter.IsInternalUser(email);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotIndicateInternalIfIsForbidden()
        {
            // Arrange
            var email = "john.doe@networkperspective.io";
            var forbiddenEmails = new[] { email };
            var filter = new EmailFilter(null, forbiddenEmails);

            // Act
            var result = filter.IsInternalUser(email);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldNotIndicateInternalEmptyEmail()
        {
            // Arrange
            var email = string.Empty;
            var filter = new EmailFilter(Array.Empty<string>(), Array.Empty<string>());

            // Act
            var result = filter.IsInternalUser(email);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldNotIndicateInternalNullEmail()
        {
            // Arrange
            string email = null;
            var filter = new EmailFilter(Array.Empty<string>(), Array.Empty<string>());

            // Act
            var result = filter.IsInternalUser(email);

            // Assert
            result.Should().BeFalse();
        }
    }
}