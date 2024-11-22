using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

internal class SlackHttpClient(HttpClient httpClient, ILogger<SlackHttpClient> logger) : ISlackHttpClient
{
    public void Dispose()
    {
        httpClient?.Dispose();
    }

    public Task<T> GetAsync<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError
        => Invoke<T>(() => httpClient.GetAsync(path, stoppingToken));

    public Task<T> PostAsync<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError
        => Invoke<T>(() => httpClient.PostAsync(path, null, stoppingToken));

    public Task<T> PostAsync<T>(string path, HttpContent content, CancellationToken stoppingToken = default) where T : IResponseWithError
        => Invoke<T>(() => httpClient.PostAsync(path, content, stoppingToken));

    public Task<T> SendAsync<T>(HttpRequestMessage requestMessage, CancellationToken stoppingToken = default) where T : IResponseWithError
        => Invoke<T>(() => httpClient.SendAsync(requestMessage, stoppingToken));

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