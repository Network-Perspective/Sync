using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients
{
    internal class SlackHttpClient : ISlackHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SlackHttpClient> _logger;

        public SlackHttpClient(HttpClient httpClient, ILogger<SlackHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public Task<T> GetAsync<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError
            => Invoke<T>(() => _httpClient.GetAsync(path, stoppingToken));

        public Task<T> PostAsync<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError
            => Invoke<T>(() => _httpClient.PostAsync(path, null, stoppingToken));

        public Task<T> PostAsync<T>(string path, HttpContent content, CancellationToken stoppingToken = default) where T : IResponseWithError
            => Invoke<T>(() => _httpClient.PostAsync(path, content, stoppingToken));

        private static async Task<T> Invoke<T>(Func<Task<HttpResponseMessage>> innerMethod) where T : IResponseWithError
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