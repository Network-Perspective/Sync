using System;
using System.Threading;
using System.Threading.Tasks;

using Polly.Retry;

namespace NetworkPerspective.Sync.Infrastructure.Google.Extensions
{
    internal static class AsyncRetryPolicyExtensions
    {
        public static Task<TOutput> ExecuteAsync<TOutput>(this AsyncRetryPolicy retryPolicy, Func<CancellationToken, Task<TOutput>> func, CancellationToken stoppingToken)
            => retryPolicy.ExecuteAsync(func, stoppingToken);
    }
}