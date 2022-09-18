using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;

namespace NetworkPerspective.Sync.Infrastructure.Slack
{
    internal class SlackFacadeFactory : IDataSourceFactory
    {
        private readonly INetworkService _networkService;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHashingServiceFactory _hashingServiceFactory;
        private readonly ILoggerFactory _loggerFactory;

        public SlackFacadeFactory(INetworkService networkService,
                                  ISecretRepositoryFactory secretRepositoryFactory,
                                  IHttpClientFactory httpClientFactory,
                                  IHashingServiceFactory hashingServiceFactory,
                                  ILoggerFactory loggerFactory)
        {
            _networkService = networkService;
            _secretRepositoryFactory = secretRepositoryFactory;
            _httpClientFactory = httpClientFactory;
            _hashingServiceFactory = hashingServiceFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);

            var logger = _loggerFactory.CreateLogger<SlackFacade>();
            var paginationHandler = new CursorPaginationHandler(_loggerFactory.CreateLogger<CursorPaginationHandler>());

            var employeeProfileClient = new MembersClient(_loggerFactory.CreateLogger<MembersClient>());
            var chatClient = new ChatClient(_loggerFactory.CreateLogger<ChatClient>());

            return new SlackFacade(_networkService, secretRepository, employeeProfileClient, chatClient, _hashingServiceFactory, _httpClientFactory, paginationHandler, logger);
        }
    }
}