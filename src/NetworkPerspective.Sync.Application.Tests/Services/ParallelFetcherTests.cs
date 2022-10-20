using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Extensions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class ParallelFetcherTests
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
            await ParallelFetcher.FetchAsync(userIds, StatusReportCallback, EmptySingleFetchTask);

            // Assert
            completionRateReports.Should().BeEquivalentTo(new[] { 0.0, 20.0, 40.0, 60.0, 80.0, 100.0 });
        }

        [Fact]
        public void ShouldRunInParallel()
        {
            // Arrange
            var userIds = new[] { "id1", "id2", "id3", "id4" };

            static async Task<ISet<Interaction>> SingleFetchTask(string id)
            {
                await Task.Delay(50);
                return ImmutableHashSet<Interaction>.Empty;
            }

            // Act
            Func<Task<ISet<Interaction>>> func = () => ParallelFetcher.FetchAsync(userIds, EmptyStatusReportCallback, SingleFetchTask);

            // Assert
            func.ExecutionTime().Should().BeLessThan(150.Milliseconds());
        }

        [Fact]
        public async void ShouldCollectAllInteractions()
        {
            // Arrange
            var userIds = new[] { "id1", "id2", "id3", "id4" };

            static Task<ISet<Interaction>> SingleFetchTask(string id)
            {
                var interaction = Interaction.CreateEmail(DateTime.UtcNow, Employee.CreateBot(id), Employee.CreateBot(id), string.Empty);
                var interactions = new HashSet<Interaction> { interaction } as ISet<Interaction>;
                return Task.FromResult(interactions);
            }

            // Act
            var result = await ParallelFetcher.FetchAsync(userIds, EmptyStatusReportCallback, SingleFetchTask);

            // Assert
            result.Should().HaveCount(userIds.Length);
        }

        private static Task EmptyStatusReportCallback(double completionRate)
            => Task.CompletedTask;

        private static Task<ISet<Interaction>> EmptySingleFetchTask(string id)
                => Task.FromResult(ImmutableHashSet<Interaction>.Empty as ISet<Interaction>);
    }
}