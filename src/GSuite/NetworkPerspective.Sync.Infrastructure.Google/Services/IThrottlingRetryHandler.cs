using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal interface IThrottlingRetryHandler
    {
        Task<TOutput> ExecuteAsync<TOutput>(Func<CancellationToken, Task<TOutput>> func, ILogger logger, CancellationToken stoppingToken);
    }

    internal class ThrottlingRetryHandler : IThrottlingRetryHandler
    {
        public async Task<TOutput> ExecuteAsync<TOutput>(Func<CancellationToken, Task<TOutput>> func, ILogger logger, CancellationToken stoppingToken)
        {
            var policy = CreateRetryPolicy(logger);

            return await policy.ExecuteAsync(func, stoppingToken);
        }

        private AsyncRetryPolicy CreateRetryPolicy(ILogger logger)
        {
            void OnThrottingException(Exception exception, TimeSpan timeSpan)
            {
                logger.LogDebug("Google api threw an exception that suggests queries have been throttled. " +
                    "The attempt will be retried in {timespan}s", timeSpan.TotalSeconds);
            }

            return Policy
                .Handle<GoogleApiException>(IsThrottlingException)
                .WaitAndRetryForeverAsync(SleepDurationProvider, OnThrottingException);
        }

        private static bool IsThrottlingException(GoogleApiException exception)
        {
            const string exceptionDomain = "usageLimits";
            const string exceptionReason = "rateLimitExceeded";
            return exception.Error.Errors.Any(x => x.Domain == exceptionDomain && x.Reason == exceptionReason);
        }

        private static TimeSpan SleepDurationProvider(int retryCount)
            => TimeSpan.FromMinutes(1);
    }
}