using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Exceptions;

using Newtonsoft.Json;

using Polly;
using Polly.Retry;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal class ApiClientBase : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClientBase> _logger;

        public ApiClientBase(HttpClient httpClient, ILogger<ApiClientBase> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        protected Task<T> Get<T>(string path, CancellationToken stoppingToken) where T : IResponseWithError
            => Invoke<T>(() => _httpClient.GetAsync(path, stoppingToken));

        protected Task<T> Post<T>(string path, CancellationToken stoppingToken) where T : IResponseWithError
            => Invoke<T>(() => _httpClient.PostAsync(path, null, stoppingToken));

        protected Task<T> Post<T>(string path, HttpContent content, CancellationToken stoppingToken) where T : IResponseWithError
            => Invoke<T>(() => _httpClient.PostAsync(path, content, stoppingToken));

        private async Task<T> Invoke<T>(Func<Task<HttpResponseMessage>> innerMethod) where T : IResponseWithError
        {
            var policy = CreateRetryPolicyOnTransientError();

            var method = () => InnerInvoke<T>(innerMethod);

            return await policy.ExecuteAsync(method);
        }

        private AsyncRetryPolicy CreateRetryPolicyOnTransientError()
        {
            static bool TransientProblem(ApiException apiException)
            {
                var transientErrors = new[]
                {
                    SlackApiErrorCodes.FatalError,
                    SlackApiErrorCodes.InternalError,
                    SlackApiErrorCodes.ServiceUnavailable,
                    SlackApiErrorCodes.RequestTimeout
                };

                return transientErrors.Contains(apiException.ApiErrorCode);
            }

            void OnTransientProblem(Exception exception, TimeSpan timeSpan)
            {
                var apiException = exception as ApiException;
                _logger.LogDebug("Failed to call Slack Api due to '{errorCode}'. Next attempt in {timespan}", apiException.ApiErrorCode, timeSpan);
            }

            static TimeSpan SleepDurationProvider(int retryCount)
                => TimeSpan.FromSeconds(Math.Pow(2, retryCount));

            return Policy
                .Handle<ApiException>(TransientProblem)
                .WaitAndRetryAsync(6, SleepDurationProvider, OnTransientProblem);
        }

        private async Task<T> InnerInvoke<T>(Func<Task<HttpResponseMessage>> innerMethod) where T : IResponseWithError
        {
            var result = await innerMethod.Invoke();
            var responseBody = await result.Content.ReadAsStringAsync();

            var responseObject = JsonConvert.DeserializeObject<T>(responseBody);

            if (responseObject.IsOk == false)
                throw new ApiException((int)result.StatusCode, responseObject.Error);

            return responseObject;
        }
    }
}