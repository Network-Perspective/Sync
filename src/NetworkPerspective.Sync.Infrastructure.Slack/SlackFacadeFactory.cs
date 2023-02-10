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
        private readonly ISlackHttpClientFactory _slackHttpClientFactory;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly IClock _clock;
        private readonly ILoggerFactory _loggerFactory;

        public SlackFacadeFactory(INetworkService networkService,
                                  ISlackHttpClientFactory slackHttpClientFactory,
                                  ITasksStatusesCache tasksStatusesCache,
                                  IClock clock,
                                  ILoggerFactory loggerFactory)
        {
            _networkService = networkService;
            _slackHttpClientFactory = slackHttpClientFactory;
            _tasksStatusesCache = tasksStatusesCache;
            _clock = clock;
            _loggerFactory = loggerFactory;
        }

        public Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var paginationHandler = new CursorPaginationHandler(_loggerFactory.CreateLogger<CursorPaginationHandler>());

            var employeeProfileClient = new MembersClient(_loggerFactory.CreateLogger<MembersClient>());
            var chatClient = new ChatClient(_tasksStatusesCache, _loggerFactory.CreateLogger<ChatClient>());

            var facade = new SlackFacade(_networkService, employeeProfileClient, chatClient, _slackHttpClientFactory, paginationHandler, _clock, _loggerFactory.CreateLogger<SlackFacade>());
            return Task.FromResult(facade as IDataSource);
        }
    }
}