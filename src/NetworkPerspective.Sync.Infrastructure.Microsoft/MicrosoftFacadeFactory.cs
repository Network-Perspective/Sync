using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    internal class MicrosoftFacadeFactory : IDataSourceFactory
    {
        private readonly IMicrosoftClientFactory _microsoftClientFactory;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILoggerFactory _loggerFactory;

        public MicrosoftFacadeFactory(
            IMicrosoftClientFactory microsoftClientFactory,
            ITasksStatusesCache tasksStatusesCache,
            ILoggerFactory loggerFactory)
        {
            _microsoftClientFactory = microsoftClientFactory;
            _tasksStatusesCache = tasksStatusesCache;
            _loggerFactory = loggerFactory;
        }

        public async Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var microsoftClient = await _microsoftClientFactory.GetMicrosoftClientAsync(networkId, stoppingToken);

            var usersClient = new UsersClient(microsoftClient, _loggerFactory.CreateLogger<UsersClient>());
            var mailboxClient = new MailboxClient(microsoftClient, _tasksStatusesCache, _loggerFactory.CreateLogger<MailboxClient>());
            var calendarClient = new CalendarClient(microsoftClient, _tasksStatusesCache, _loggerFactory.CreateLogger<CalendarClient>());
            return new MicrosoftFacade(usersClient, mailboxClient, calendarClient, _loggerFactory);
        }
    }
}