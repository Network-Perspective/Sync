using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests;

public class ErrorServiceTests
{
    [Fact]
    public void ShouldReturn400OnTakCancelled()
    {
        // Arrange
        var errorService = new ErrorService(NullLogger<ErrorService>.Instance);

        // Act
        var result = errorService.MapToError(new TaskCanceledException());

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}