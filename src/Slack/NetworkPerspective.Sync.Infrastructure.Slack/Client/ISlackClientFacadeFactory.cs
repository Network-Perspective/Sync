using System.Threading;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;

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
        private readonly ISlackHttpClientFactory _slackHttpClientFactory;
        private readonly CursorPaginationHandler _cursorPaginationHandler;

        public SlackClientFacadeFactory(ISlackHttpClientFactory slackHttpClientFactory, CursorPaginationHandler cursorPaginationHandler)
        {
            _slackHttpClientFactory = slackHttpClientFactory;
            _cursorPaginationHandler = cursorPaginationHandler;
        }

        public ISlackClientBotScopeFacade CreateWithBotToken(CancellationToken stoppingToken = default)
        {
            var slackClient = _slackHttpClientFactory.CreateWithBotToken();

            return new SlackClientBotScopeFacade(slackClient, _cursorPaginationHandler);
        }

        public ISlackClientUserScopeFacade CreateWithUserToken(CancellationToken stoppingToken = default)
        {
            var slackClient = _slackHttpClientFactory.CreateWithUserToken();

            return new SlackClientUserScopeFacade(slackClient, _cursorPaginationHandler);
        }

        public ISlackClientUnauthorizedFacade CreateUnauthorized()
        {
            var slackHttpClient = _slackHttpClientFactory.Create();

            return new SlackClientUnauthorizedFacade(slackHttpClient);
        }
    }
}