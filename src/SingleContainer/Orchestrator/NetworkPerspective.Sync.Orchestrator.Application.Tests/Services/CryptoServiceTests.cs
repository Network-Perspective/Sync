using System;

using FluentAssertions;

using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Services;

public class CryptoServiceTests
{
    [Fact]
    public void ShouldGenerateSalt()
    {
        // Arrange
        var service = new CryptoService();

        // Act
        var salt = service.GenerateSalt();

        // Assert
        salt.Should().NotBeNull();
        salt.Length.Should().Be(16);
    }

    [Fact]
    public void ShouldGenerateDifferentSalt()
    {
        // Arrange
        var service = new CryptoService();

        // Act
        var salt1 = service.GenerateSalt();
        var salt2 = service.GenerateSalt();

        // Assert
        salt1.Should().NotBeSameAs(salt2);
    }

    [Fact]
    public void ShouldhashPasword()
    {
        // Arrange
        var service = new CryptoService();

        var password = "MyPassword123";
        var salt = new byte[16];

        // Act
        var hash = service.HashPassword(password, salt);

        // Assert
        hash.Should().NotBeNull();
        hash.Length.Should().Be(32);
    }

    [Fact]
    public void ShouldReturnTrueOnCorrectPassword()
    {
        // Arrange
        var password = "MyPassword123";
        var salt = new byte[16]; // Mock salt
        var service = new CryptoService();
        var hash = service.HashPassword(password, salt);

        // Act
        var isValid = service.VerifyPassword(password, Convert.ToBase64String(hash), Convert.ToBase64String(salt));

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldReturnFalseOnInvalidPassword()
    {
        // Arrange
        var password = "MyPassword123";
        var salt = new byte[16];
        var service = new CryptoService();
        var hash = service.HashPassword(password, salt);

        // Act
        var isValid = service.VerifyPassword("WrongPassword", Convert.ToBase64String(hash), Convert.ToBase64String(salt));

        // Assert
        isValid.Should().BeFalse();
    }
}