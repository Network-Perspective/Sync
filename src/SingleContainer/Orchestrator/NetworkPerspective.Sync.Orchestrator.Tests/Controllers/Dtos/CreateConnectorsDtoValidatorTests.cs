using System;

using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.Controllers.Dtos;

public class CreateConnectorsDtoValidatorTests
{
    [Fact]
    public void ShouldFindNoErrorsOnValid()
    {
        // Arrange
        var dto = new CreateConnectorDto
        {
            Id = Guid.NewGuid(),
            NetworkId = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            Type = "Slack",
            AccessToken = Guid.NewGuid().ToString(),
            Properties = []
        };

        // Act
        var result = new CreateConnectorDto.Validator().Validate(dto);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ShouldReturnErrorsOnInvalid()
    {
        // Arrange
        var dto = new CreateConnectorDto
        {
            Type = "Slack_",
            AccessToken = Guid.NewGuid().ToString(),
            Properties = []
        };

        // Act
        var result = new CreateConnectorDto.Validator().Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateConnectorDto.Id));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateConnectorDto.NetworkId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateConnectorDto.WorkerId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateConnectorDto.Type));
    }
}