using System;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Extensions
{
    public class ExceptionExtensionsTests
    {
        [Fact]
        public void ShouldReturnTrueOnTaskCancelledException()
        {
            // Arrange
            var exception = new TaskCanceledException();

            // Act
            var result = exception.IndicatesTaskCanceled();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrueOnTaskCancelledExceptionAsInternalException()
        {
            // Arrange
            var exception = new Exception("foo", new Exception("bar", new TaskCanceledException()));

            // Act
            var result = exception.IndicatesTaskCanceled();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalseOnOtherExceptionType()
        {
            // Arrange
            var exception = new ArgumentException("foo", new ArgumentOutOfRangeException());

            // Act
            var result = exception.IndicatesTaskCanceled();

            // Assert
            result.Should().BeFalse();
        }
    }
}