using System;

using Google.Apis.Auth.OAuth2.Responses;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

using Polly;
using Polly.Retry;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal static class RetryPolicy
    {
        public static AsyncRetryPolicy CreateSecretRotationRetryPolicy(ILogger logger)
        {
            void OnTokenRotationException(Exception exception, int count)
            {
                logger.LogDebug("Google API threw an exception that suggests the opration got interrupted by secret rotation. Retrying...");
            }

            return Policy
                .Handle<TokenResponseException>(x => x.Error.IsInvalidSignatureError())
                .RetryAsync(1, OnTokenRotationException);
        }
    }
}