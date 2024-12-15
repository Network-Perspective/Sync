using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Configs;

using Polly;
using Polly.Retry;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

internal class ResilientSlackHttpClientDecorator : ISlackHttpClient
{
    private readonly ISlackHttpClient _client;
    private readonly Resiliency _resiliencyOptions;
    private readonly ILogger<ResilientSlackHttpClientDecorator> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ResilientSlackHttpClientDecorator(ISlackHttpClient client, Resiliency resiliencyOptions, ILogger<ResilientSlackHttpClientDecorator> logger)
    {
        _client = client;
        _resiliencyOptions = resiliencyOptions;
        _logger = logger;
        _retryPolicy = CreateRetryPolicyOnTransientError();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public async Task<T> GetAsync<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError
    {
        Task<T> Method(CancellationToken ct)
            => _client.GetAsync<T>(path, stoppingToken);

        return await _retryPolicy.ExecuteAsync(Method, stoppingToken);
    }

    public async Task<T> PostAsync<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError
    {
        Task<T> Method(CancellationToken ct)
            => _client.PostAsync<T>(path, stoppingToken);

        return await _retryPolicy.ExecuteAsync(Method, stoppingToken);
    }

    public async Task<T> PostAsync<T>(string path, HttpContent content, CancellationToken stoppingToken = default) where T : IResponseWithError
    {
        Task<T> Method(CancellationToken ct)
            => _client.PostAsync<T>(path, content, stoppingToken);

        return await _retryPolicy.ExecuteAsync(Method, stoppingToken);
    }

    public async Task<T> SendAsync<T>(HttpRequestMessage requestMessage, CancellationToken stoppingToken = default) where T : IResponseWithError
    {
        Task<T> Method(CancellationToken ct)
            => _client.SendAsync<T>(requestMessage, stoppingToken);

        return await _retryPolicy.ExecuteAsync(Method, stoppingToken);
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

        return Policy
            .Handle<ApiException>(TransientProblem)
            .WaitAndRetryAsync(_resiliencyOptions.Retries, OnTransientProblem);
    }
}