using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal interface ISlackClientFacadeFactory
    {
        Task<ISlackClientBotScopeFacade> CreateWithBotTokenAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task<ISlackClientUserScopeFacade> CreateWithUserTokenAsync(Guid networkId, CancellationToken stoppingToken = default);
        ISlackClientUnauthorizedFacade CreateUnauthorized();
    }

    internal class SlackClientFacadeFactory : ISlackClientFacadeFactory
    {
        private readonly ISlackHttpClientFactory _slackHttpClientFactory;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly ILoggerFactory _loggerFactory;

        public SlackClientFacadeFactory(ISlackHttpClientFactory slackHttpClientFactory, ISecretRepositoryFactory secretRepositoryFactory, ILoggerFactory loggerFactory)
        {
            _slackHttpClientFactory = slackHttpClientFactory;
            _secretRepositoryFactory = secretRepositoryFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task<ISlackClientBotScopeFacade> CreateWithBotTokenAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var slackClient = await CreateSlackClientAsync(networkId, SlackKeys.TokenKeyPattern, stoppingToken);
            var paginationHandler = new CursorPaginationHandler(_loggerFactory.CreateLogger<CursorPaginationHandler>());
            return new SlackClientBotScopeFacade(slackClient, paginationHandler);
        }

        public async Task<ISlackClientUserScopeFacade> CreateWithUserTokenAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var slackClient = await CreateSlackClientAsync(networkId, SlackKeys.UserTokenKeyPattern, stoppingToken);
            var paginationHandler = new CursorPaginationHandler(_loggerFactory.CreateLogger<CursorPaginationHandler>());
            return new SlackClientUserScopeFacade(slackClient, paginationHandler);
        }

        public ISlackClientUnauthorizedFacade CreateUnauthorized()
        {
            var slackHttpClient = _slackHttpClientFactory.Create();

            return new SlackClientUnauthorizedFacade(slackHttpClient);
        }

        private async Task<ISlackHttpClient> CreateSlackClientAsync(Guid networkId, string tokenKeyPattern, CancellationToken stoppingToken)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);
            var tokenKey = string.Format(tokenKeyPattern, networkId);
            var token = await secretRepository.GetSecretAsync(tokenKey, stoppingToken);

            return _slackHttpClientFactory.CreateWithToken(token);
        }
    }
}