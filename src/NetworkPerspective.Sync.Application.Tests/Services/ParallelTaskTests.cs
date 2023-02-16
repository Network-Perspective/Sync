using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Extensions;

using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class ParallelTaskTests
    {
        [Fact]
        public async Task ShouldUpdateStatus()
        {
            // Arrange
            var userIds = new[] { "id1", "id2", "id3", "id4", "id5" };

            var completionRateReports = new List<double>();

            Task StatusReportCallback(double completionRate)
            {
                completionRateReports.Add(completionRate);
                return Task.CompletedTask;
            }

            // Act
            await ParallelTask.RunAsync(userIds, StatusReportCallback, EmptySingleFetchTask);

            // Assert
            completionRateReports.Should().BeEquivalentTo(new[] { 0.0, 20.0, 40.0, 60.0, 80.0, 100.0 });
        }

        [Fact]
        public void ShouldRunInParallel()
        {
            // Arrange
            var userIds = Enumerable
                .Range(0, 4)
                .Select(x => x.ToString());

            static async Task SingleFetchTask(string id)
            {
                await Task.Delay(50);
            }

            // Act
            Func<Task> func = () => ParallelTask.RunAsync(userIds, EmptyStatusReportCallback, SingleFetchTask);

            // Assert
            func.ExecutionTime().Should().BeLessThan(150.Milliseconds());
        }

        [Fact]
        public async void ShouldCollectAllInteractions()
        {
            // Arrange
            var userIds = Enumerable
                .Range(0, 4)
                .Select(x => x.ToString());

            var invocationCount = 0;

            Task SingleFetchTask(string id)
            {
                Interlocked.Increment(ref invocationCount);
                return Task.CompletedTask;
            }

            // Act
            await ParallelTask.RunAsync(userIds, EmptyStatusReportCallback, SingleFetchTask);

            // Assert
            invocationCount.Should().Be(userIds.Count());
        }

        private static Task EmptyStatusReportCallback(double completionRate)
            => Task.CompletedTask;

        private static Task EmptySingleFetchTask(string id)
                => Task.CompletedTask;
    }
}