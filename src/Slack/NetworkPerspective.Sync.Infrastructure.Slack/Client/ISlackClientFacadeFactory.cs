using System.Net.Http;
using System.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal interface ISlackClientFacadeFactory
    {
        ISlackClientBotScopeFacade CreateWithBotToken(CancellationToken stoppingToken = default);
        ISlackClientUserScopeFacade CreateWithUserToken(CancellationToken stoppingToken = default);
        ISlackClientUnauthorizedFacade CreateUnauthorized();
    }

    internal class SlackClientFacadeFactory : ISlackClientFacadeFactory
    {
        private readonly Resiliency _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CursorPaginationHandler _cursorPaginationHandler;

        public SlackClientFacadeFactory(IOptions<Resiliency> options, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, CursorPaginationHandler cursorPaginationHandler)
        {
            _options = options.Value;
            _loggerFactory = loggerFactory;
            _httpClientFactory = httpClientFactory;
            _cursorPaginationHandler = cursorPaginationHandler;
        }

        public ISlackClientBotScopeFacade CreateWithBotToken(CancellationToken stoppingToken = default)
        {
            var slackClient = CreateUsingClientName(Consts.SlackApiHttpClientWithBotTokenName);

            return new SlackClientBotScopeFacade(slackClient, _cursorPaginationHandler);
        }

        public ISlackClientUserScopeFacade CreateWithUserToken(CancellationToken stoppingToken = default)
        {
            var slackClient = CreateUsingClientName(Consts.SlackApiHttpClientWithUserTokenName);

            return new SlackClientUserScopeFacade(slackClient, _cursorPaginationHandler);
        }

        public ISlackClientUnauthorizedFacade CreateUnauthorized()
        {
            var slackHttpClient = CreateUsingClientName(Consts.SlackApiHttpClientName);

            return new SlackClientUnauthorizedFacade(slackHttpClient);
        }

        private ISlackHttpClient CreateUsingClientName(string clientName)
        {
            var httpClient = _httpClientFactory.CreateClient(clientName);

            var slackHttpClient = new SlackHttpClient(httpClient, _loggerFactory.CreateLogger<SlackHttpClient>());
            return new ResilientSlackHttpClientDecorator(slackHttpClient, _options, _loggerFactory.CreateLogger<ResilientSlackHttpClientDecorator>());
        }
    }
}