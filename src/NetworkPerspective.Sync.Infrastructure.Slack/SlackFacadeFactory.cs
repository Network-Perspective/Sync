using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;

namespace NetworkPerspective.Sync.Infrastructure.Slack
{
    internal class SlackFacadeFactory : IDataSourceFactory
    {
        private readonly INetworkService _networkService;
        private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly IClock _clock;
        private readonly ILoggerFactory _loggerFactory;

        public SlackFacadeFactory(INetworkService networkService,
                                  ISlackClientFacadeFactory slackClientFacadeFactory,
                                  ITasksStatusesCache tasksStatusesCache,
                                  IClock clock,
                                  ILoggerFactory loggerFactory)
        {
            _networkService = networkService;
            _slackClientFacadeFactory = slackClientFacadeFactory;
            _tasksStatusesCache = tasksStatusesCache;
            _clock = clock;
            _loggerFactory = loggerFactory;
        }

        public Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var membersClient = new MembersClient(_loggerFactory.CreateLogger<MembersClient>());
            var chatClient = new ChatClient(_tasksStatusesCache, _loggerFactory.CreateLogger<ChatClient>());

            var facade = new SlackFacade(_networkService, membersClient, chatClient, _slackClientFacadeFactory, _clock, _loggerFactory);
            return Task.FromResult(facade as IDataSource);
        }
    }
}