using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain
{
    public class BatcherTests
    {
        [Fact]
        public async Task ShouldInvokeCallback()
        {
            // Arrange
            const int size = 2;

            var result = new List<int>();

            Task Callback(BatchReadyEventArgs<int> args)
            {
                result.AddRange(args.BatchItems);
                return Task.CompletedTask;
            };

            var buffer = new Batcher<int>(size);
            buffer.OnBatchReady(Callback);

            var items = Enumerable.Range(0, 5);

            // Act
            await buffer.AddRangeAsync(items);

            // Assert
            result.Should().BeEquivalentTo(Enumerable.Range(0, 4));
        }

        [Fact]
        public async Task ShouldFlush()
        {
            // Arrange
            const int size = 2;

            var result = new List<int>();

            Task Callback(BatchReadyEventArgs<int> args)
            {
                result.AddRange(args.BatchItems);
                return Task.CompletedTask;
            };

            var buffer = new Batcher<int>(size);
            buffer.OnBatchReady(Callback);

            var items = Enumerable.Range(0, 5);
            await buffer.AddRangeAsync(items);

            // Act
            await buffer.FlushAsync();
            
            // Assert
            result.Should().BeEquivalentTo(Enumerable.Range(0, 5));
        }

        [Fact]
        public void ShouldThrowOnInvalidSize()
        {
            // Arrange
            Action action = () => new Batcher<int>(0);

            // Act Assert
            action.Should().Throw<ArgumentException>();
        }

    }
}
