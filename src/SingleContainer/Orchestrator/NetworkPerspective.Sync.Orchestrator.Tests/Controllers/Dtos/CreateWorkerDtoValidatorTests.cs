using System;

using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.Controllers.Dtos;

public class CreateWorkerDtoValidatorTests
{
    [Fact]
    public void ShouldFindNoErrorsOnValid()
    {
        // Arrange
        var dto = new CreateWorkerDto
        {
            Id = Guid.NewGuid(),
            Name = "Name-subname_123",
        };

        // Act
        var result = new CreateWorkerDto.Validator().Validate(dto);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ShouldReturnErrorsOnInvalid()
    {
        // Arrange
        var dto = new CreateWorkerDto
        {
            Id = Guid.Empty,
            Name = "Name-subname_123<",
        };

        // Act
        var result = new CreateWorkerDto.Validator().Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateWorkerDto.Id));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateWorkerDto.Name));
    }
}