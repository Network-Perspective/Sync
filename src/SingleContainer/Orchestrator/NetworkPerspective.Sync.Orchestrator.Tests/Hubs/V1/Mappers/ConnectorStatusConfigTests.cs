using FluentAssertions;

using Mapster;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.Hubs.V1.Mappers;

public class ConnectorStatusConfigTests
{
    [Fact]
    public void ShouldMapRunning()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        new ConnectorStatusConfig().Register(config);

        var input = new ConnectorStatusResponse
        {
            IsAuthorized = true,
            IsRunning = true,
            CurrentTaskCaption = "caption",
            CurrentTaskCompletionRate = 42,
            CurrentTaskDescription = "description",
        };

        // Act
        var result = input.Adapt<ConnectorStatus>(config);

        // Assert
        result.IsAuthorized.Should().Be(input.IsAuthorized);
        result.IsRunning.Should().Be(input.IsRunning);
        result.CurrentTask.Caption.Should().Be(input.CurrentTaskCaption);
        result.CurrentTask.Description.Should().Be(input.CurrentTaskDescription);
        result.CurrentTask.CompletionRate.Should().Be(input.CurrentTaskCompletionRate);
    }

    [Fact]
    public void ShouldMapIdle()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        new ConnectorStatusConfig().Register(config);

        var input = new ConnectorStatusResponse
        {
            IsAuthorized = true,
            IsRunning = false,
            CurrentTaskCaption = null,
            CurrentTaskCompletionRate = null,
            CurrentTaskDescription = null,
        };

        // Act
        var result = input.Adapt<ConnectorStatus>(config);

        // Assert
        result.IsAuthorized.Should().Be(input.IsAuthorized);
        result.IsRunning.Should().Be(input.IsRunning);
        result.CurrentTask.Caption.Should().Be(string.Empty);
        result.CurrentTask.Description.Should().Be(string.Empty);
        result.CurrentTask.CompletionRate.Should().Be(null);
    }
}