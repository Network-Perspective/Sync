using System;

using FluentAssertions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Controllers;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Tests.Controllers;

public class ErrorControllerTests
{
    private readonly Mock<IErrorService> _errorServiceMock = new();

    public ErrorControllerTests()
    {
        _errorServiceMock.Reset();
    }

    [Fact]
    public void ShouldInvokeErrorServiceWithException()
    {
        // Arrange
        const string type = "type";
        const string title = "title";
        const string details = "details";
        const int statusCode = 200;

        var exception = new Exception();
        var exceptionHandlerFeature = new ExceptionHandlerFeature
        {
            Error = exception
        };

        var features = new FeatureCollection();
        features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
        features.Set<IHttpResponseFeature>(new HttpResponseFeature());

        var controller = new ErrorController(_errorServiceMock.Object);
        controller.ControllerContext.HttpContext = new DefaultHttpContext(features);

        _errorServiceMock
            .Setup(x => x.MapToError(exception))
            .Returns(new Error(type, title, details, 200));

        // Act
        var result = controller.HandleException();

        // Assert
        result.Type.Should().Be(type);
        result.Title.Should().Be(title);
        result.Detail.Should().Be(details);
        result.Status.Should().Be(statusCode);

        _errorServiceMock.Verify(x => x.MapToError(exception), Times.Once());
    }
}