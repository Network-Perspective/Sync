using System;
using System.Linq;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Sync;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Sync
{
    public class SyncResultTests
    {

        [Fact]
        public void ShouldSetProperties()
        {
            // Arrange
            var usersCount = 10;
            var interactionsCount = 42;
            var exceptions = new[]
            {
                new Exception("foo"),
                new Exception("bar")
            };

            // Act
            var result = new SyncResult(usersCount, interactionsCount, exceptions);

            // Assert
            result.TasksCount.Should().Be(usersCount);
            result.FailedTasksCount.Should().Be(2);
            result.TotalInteractionsCount.Should().Be(interactionsCount);
            result.SuccessRate.Should().Be(80.0);
            result.Exceptions.Should().BeEquivalentTo(exceptions);
        }

        [Fact]
        public void ShouldCombineResults()
        {
            // Arrange
            var usersCount1 = 10;
            var interactionsCount1 = 55;
            var exceptions1 = new[]
            {
                new Exception("foo"),
                new Exception("bar")
            };

            var usersCount2 = 10;
            var interactionsCount2 = 42;
            var exceptions2 = new[]
            {
                new Exception("foo"),
                new Exception("bar"),
                new Exception("baz")
            };
            var result1 = new SyncResult(usersCount1, interactionsCount1, exceptions1);
            var result2 = new SyncResult(usersCount2, interactionsCount2, exceptions2);

            // Act
            var result = SyncResult.Combine(result1, result2);

            // Assert
            result.TasksCount.Should().Be(usersCount1 + usersCount2);
            result.FailedTasksCount.Should().Be(5);
            result.TotalInteractionsCount.Should().Be(interactionsCount1 + interactionsCount2);
            result.SuccessRate.Should().Be(75.0);
            result.Exceptions.Should().BeEquivalentTo(exceptions1.Union(exceptions2));
        }

    }
}