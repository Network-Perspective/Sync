using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Extensions;

using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Common.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Sync
{
    public class ParallelSyncTaskTests
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
            await ParallelSyncTask.RunAsync(userIds, StatusReportCallback, EmptySingleFetchTask);

            // Assert
            completionRateReports.Should().BeEquivalentTo(new[] { 0.0, 20.0, 40.0, 60.0, 80.0, 100.0 });
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public void ShouldRunInParallel()
        {
            // Arrange
            var userIds = Enumerable
                .Range(0, 4)
                .Select(x => x.ToString());

            static async Task<SingleTaskResult> SingleFetchTask(string id)
            {
                await Task.Delay(50);
                return new SingleTaskResult(42);
            }

            // Act
            Func<Task> func = () => ParallelSyncTask.RunAsync(userIds, EmptyStatusReportCallback, SingleFetchTask);

            // Assert
            func.ExecutionTime().Should().BeLessThan(150.Milliseconds());
        }

        [Fact]
        public async Task ShouldCollectAllInteractions()
        {
            // Arrange
            var userIds = Enumerable
                .Range(0, 4)
                .Select(x => x.ToString());

            var invocationCount = 0;

            Task<SingleTaskResult> SingleFetchTask(string id)
            {
                Interlocked.Increment(ref invocationCount);
                return Task.FromResult(new SingleTaskResult(42));
            }

            // Act
            await ParallelSyncTask.RunAsync(userIds, EmptyStatusReportCallback, SingleFetchTask);

            // Assert
            invocationCount.Should().Be(userIds.Count());
        }

        [Fact]
        public async Task ShouldReturnResult()
        {
            // Arrange
            var userIds = Enumerable
                .Range(0, 4)
                .Select(x => x.ToString());
            var exception = new Exception();

            var invocationCount = 0;
            var expectedResult = new SyncResult(userIds.Count(), 126, new[] { exception });


            Task<SingleTaskResult> SingleFetchTask(string id)
            {
                if (id == "0")
                    throw exception;

                Interlocked.Increment(ref invocationCount);
                return Task.FromResult(new SingleTaskResult(42));
            }

            // Act
            var result = await ParallelSyncTask.RunAsync(userIds, EmptyStatusReportCallback, SingleFetchTask);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        private static Task EmptyStatusReportCallback(double completionRate)
            => Task.CompletedTask;

        private static Task<SingleTaskResult> EmptySingleFetchTask(string id)
                => Task.FromResult(new SingleTaskResult(42));
    }
}