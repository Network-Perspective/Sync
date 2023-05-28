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
        Task<ISlackClientBotScopeFacade> CreateWithUserTokenAsync(Guid networkId, CancellationToken stoppingToken = default);
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

        public Task<ISlackClientBotScopeFacade> CreateWithBotTokenAsync(Guid networkId, CancellationToken stoppingToken = default)
            => CreateWithTokenAsync(networkId, SlackKeys.TokenKeyPattern, stoppingToken);

        public Task<ISlackClientBotScopeFacade> CreateWithUserTokenAsync(Guid networkId, CancellationToken stoppingToken = default)
            => CreateWithTokenAsync(networkId, SlackKeys.UserTokenKeyPattern, stoppingToken);

        public ISlackClientUnauthorizedFacade CreateUnauthorized()
        {
            var slackHttpClient = _slackHttpClientFactory.Create();

            return new SlackClientUnauthorizedFacade(slackHttpClient);
        }

        private async Task<ISlackClientBotScopeFacade> CreateWithTokenAsync(Guid networkId, string tokenKeyPattern, CancellationToken stoppingToken = default)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);
            var tokenKey = string.Format(tokenKeyPattern, networkId);
            var token = await secretRepository.GetSecretAsync(tokenKey, stoppingToken);

            var slackHttpClient = _slackHttpClientFactory.CreateWithToken(token);
            var paginationHandler = new CursorPaginationHandler(_loggerFactory.CreateLogger<CursorPaginationHandler>());

            return new SlackClientBotScopeFacade(slackHttpClient, paginationHandler);
        }
    }
}