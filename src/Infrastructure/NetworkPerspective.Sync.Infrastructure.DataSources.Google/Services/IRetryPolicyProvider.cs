﻿using System;
using System.Linq;

using Google;
using Google.Apis.Auth.OAuth2.Responses;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions;

using Polly;
using Polly.Retry;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal interface IRetryPolicyProvider
{
    AsyncRetryPolicy GetSecretRotationRetryPolicy();
    AsyncRetryPolicy GetThrottlingRetryPolicy();
}

internal class RetryPolicyProvider(ILogger<RetryPolicyProvider> logger) : IRetryPolicyProvider
{
    public AsyncRetryPolicy GetSecretRotationRetryPolicy()
    {
        void OnTokenRotationException(Exception exception, int count)
        {
            logger.LogDebug("Google API threw an exception that suggests the opration got interrupted by secret rotation. Retrying...");
        }

        return Policy
            .Handle<TokenResponseException>(x => x.Error.IsInvalidSignatureError())
            .RetryAsync(1, OnTokenRotationException);
    }


    public AsyncRetryPolicy GetThrottlingRetryPolicy()
    {
        bool IsThrottlingException(GoogleApiException exception)
        {
            const string exceptionDomain = "usageLimits";
            const string exceptionReason = "rateLimitExceeded";
            return exception.Error.Errors.Any(x => x.Domain == exceptionDomain && x.Reason == exceptionReason);
        }

        TimeSpan SleepDurationProvider(int retryCount)
            => TimeSpan.FromMinutes(1);

        void OnThrottingException(Exception exception, TimeSpan timeSpan)
        {
            logger.LogDebug("Google api threw an exception that suggests queries have been throttled. " +
                "The attempt will be retried in {timespan}s", timeSpan.TotalSeconds);
        }

        return Policy
            .Handle<GoogleApiException>(IsThrottlingException)
            .WaitAndRetryForeverAsync(SleepDurationProvider, OnThrottingException);
    }
}