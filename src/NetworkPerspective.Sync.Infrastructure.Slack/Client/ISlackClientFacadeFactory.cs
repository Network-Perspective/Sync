using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal interface ISlackClientFacadeFactory
    {
        Task<ISlackClientFacade> CreateAsync(Guid networkId, CancellationToken stoppingToken = default);
        ISlackClientFacade CreateUnauthorized();
    }

    internal class SlackClientFacadeFactory : ISlackClientFacadeFactory
    {
        private readonly ISlackHttpClientFactory _slackHttpClientFactory;
        private readonly ILoggerFactory _loggerFactory;

        public SlackClientFacadeFactory(ISlackHttpClientFactory slackHttpClientFactory, ILoggerFactory loggerFactory)
        {
            _slackHttpClientFactory = slackHttpClientFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task<ISlackClientFacade> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var slackHttpClient = await _slackHttpClientFactory.CreateAsync(networkId, stoppingToken);
            var paginationHandler = new CursorPaginationHandler(_loggerFactory.CreateLogger<CursorPaginationHandler>());

            return new SlackClientFacade(slackHttpClient, paginationHandler);
        }

        public ISlackClientFacade CreateUnauthorized()
        {
            var slackHttpClient = _slackHttpClientFactory.CreateUnauthorized();
            var paginationHandler = new CursorPaginationHandler(_loggerFactory.CreateLogger<CursorPaginationHandler>());

            return new SlackClientFacade(slackHttpClient, paginationHandler);
        }
    }
}