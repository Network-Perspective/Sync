using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Utils.Batching;

using Xunit;

namespace NetworkPerspective.Sync.Utils.Tests.Batching;

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
    public async Task ShouldStopOnCancelled()
    {
        // Arrange
        const int size = 2;

        var result = new List<int>();
        var cancellationTokenSource = new CancellationTokenSource();

        Task Callback(BatchReadyEventArgs<int> args)
        {
            result.AddRange(args.BatchItems);
            cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        };

        var buffer = new Batcher<int>(size);
        buffer.OnBatchReady(Callback);

        var items = Enumerable.Range(0, 5);

        // Act
        await buffer.AddRangeAsync(items, cancellationTokenSource.Token);

        // Assert
        result.Should().BeEquivalentTo(Enumerable.Range(0, 2));
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
    public async Task ShouldSkipCallbackOnNothingToFlush()
    {
        // Arrange
        const int size = 2;

        var invokedCallback = false;

        Task Callback(BatchReadyEventArgs<int> args)
        {
            invokedCallback = true;
            return Task.CompletedTask;
        };

        var buffer = new Batcher<int>(size);
        buffer.OnBatchReady(Callback);

        // Act
        await buffer.FlushAsync();

        // Assert
        invokedCallback.Should().BeFalse();
    }

    [Fact]
    public void ShouldThrowOnInvalidSize()
    {
        // Arrange
        Action action = () => new Batcher<int>(0);

        // Act Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task ShouldIncrementBatchNumber()
    {
        // Arrange
        const int size = 2;

        var batchNumbers = new List<int>();
        var cancellationTokenSource = new CancellationTokenSource();

        Task Callback(BatchReadyEventArgs<int> args)
        {
            batchNumbers.Add(args.BatchNumber);
            return Task.CompletedTask;
        };

        var buffer = new Batcher<int>(size);
        buffer.OnBatchReady(Callback);

        var items = Enumerable.Range(0, 5);

        // Act
        await buffer.AddRangeAsync(items, cancellationTokenSource.Token);
        await buffer.FlushAsync();

        // Assert
        batchNumbers.Should().BeEquivalentTo(Enumerable.Range(1, 3));
    }
}