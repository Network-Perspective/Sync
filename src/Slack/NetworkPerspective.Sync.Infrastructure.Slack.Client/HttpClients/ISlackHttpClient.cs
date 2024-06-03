using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients
{
    public interface ISlackHttpClient : IDisposable
    {
        Task<T> GetAsync<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError;

        Task<T> PostAsync<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError;

        Task<T> PostAsync<T>(string path, HttpContent content, CancellationToken stoppingToken = default) where T : IResponseWithError;
    }
}