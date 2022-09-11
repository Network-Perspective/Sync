using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Exceptions;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal class ApiClientBase : IDisposable
    {
        private readonly HttpClient _httpClient;

        public ApiClientBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        protected async Task<T> Invoke<T>(Func<Task<HttpResponseMessage>> innerMethod) where T : IResponseWithError
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