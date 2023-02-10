using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    public interface ISlackHttpClient : IDisposable
    {
        Task<T> Get<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError;

        Task<T> Post<T>(string path, CancellationToken stoppingToken = default) where T : IResponseWithError;

        Task<T> Post<T>(string path, HttpContent content, CancellationToken stoppingToken = default) where T : IResponseWithError;
    }
}