using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Criterias;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google
{
    internal sealed class GoogleFacadeFactory : IDataSourceFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly INetworkService _networkService;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly IClock _clock;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly IOptions<GoogleConfig> _googleConfig;

        public GoogleFacadeFactory(ILoggerFactory loggerFactory,
                                   INetworkService networkService,
                                   ISecretRepositoryFactory secretRepositoryFactory,
                                   IClock clock,
                                   ITasksStatusesCache tasksStatusesCache,
                                   IOptions<GoogleConfig> googleConfig)
        {
            _loggerFactory = loggerFactory;
            _networkService = networkService;
            _secretRepositoryFactory = secretRepositoryFactory;
            _clock = clock;
            _tasksStatusesCache = tasksStatusesCache;
            _googleConfig = googleConfig;
        }

        public async Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);

            var credentialsProvider = new CredentialsProvider(secretRepository);
            var mailboxClient = new MailboxClient(_tasksStatusesCache, _googleConfig, _loggerFactory, _clock);
            var callendarClient = new CalendarClient(_tasksStatusesCache, _googleConfig, _loggerFactory.CreateLogger<CalendarClient>());
            var employeeCriterias = new[] { new NonServiceUserCriteria(_loggerFactory.CreateLogger<NonServiceUserCriteria>()) };
            var usersClient = new UsersClient(_tasksStatusesCache, _googleConfig, employeeCriterias, _loggerFactory.CreateLogger<UsersClient>());
            var userCalendarTimeZoneReader = new UserCalendarTimeZoneReader(_googleConfig, _loggerFactory.CreateLogger<UserCalendarTimeZoneReader>());
            return new GoogleFacade(_networkService, credentialsProvider, mailboxClient, callendarClient, usersClient, userCalendarTimeZoneReader, _clock, _loggerFactory);
        }
    }
}