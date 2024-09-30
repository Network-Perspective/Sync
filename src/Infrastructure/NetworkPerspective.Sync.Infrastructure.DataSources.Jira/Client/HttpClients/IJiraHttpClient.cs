using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;

internal interface IJiraHttpClient
{
    Task<T> GetAsync<T>(string path, CancellationToken stoppingToken = default);
    Task<T> PostAsync<T>(string path, HttpContent content, CancellationToken stoppingToken = default);
}

internal class JiraHttpClient : IJiraHttpClient
{
    private readonly HttpClient _httpClient;

    public JiraHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<T> GetAsync<T>(string path, CancellationToken stoppingToken = default)
        => Invoke<T>(() => _httpClient.GetAsync(path, stoppingToken));

    public Task<T> PostAsync<T>(string path, HttpContent content, CancellationToken stoppingToken = default)
        => Invoke<T>(() => _httpClient.PostAsync(path, content, stoppingToken));

    private static async Task<T> Invoke<T>(Func<Task<HttpResponseMessage>> innerMethod)
    {
        var result = await innerMethod.Invoke();
        var responseBody = await result.Content.ReadAsStringAsync();

        if (result.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<T>(responseBody);
        else
        {
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
            throw new JiraApiException(errorResponse);
        }
    }
}