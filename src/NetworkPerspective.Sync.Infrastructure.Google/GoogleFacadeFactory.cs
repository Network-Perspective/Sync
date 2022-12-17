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
        private readonly IHashingServiceFactory _hashingServiceFactory;
        private readonly IClock _clock;
        private readonly IStatusLogger _statusLogger;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly IOptions<GoogleConfig> _googleConfig;

        public GoogleFacadeFactory(ILoggerFactory loggerFactory,
                                   INetworkService networkService,
                                   ISecretRepositoryFactory secretRepositoryFactory,
                                   IHashingServiceFactory hashingserviceFactory,
                                   IClock clock,
                                   IStatusLogger statusLogger,
                                   ITasksStatusesCache tasksStatusesCache,
                                   IOptions<GoogleConfig> googleConfig)
        {
            _loggerFactory = loggerFactory;
            _networkService = networkService;
            _secretRepositoryFactory = secretRepositoryFactory;
            _hashingServiceFactory = hashingserviceFactory;
            _clock = clock;
            _statusLogger = statusLogger;
            _tasksStatusesCache = tasksStatusesCache;
            _googleConfig = googleConfig;
        }

        public async Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);

            var network = await _networkService.GetAsync<GoogleNetworkProperties>(networkId, stoppingToken);

            var credentialsProvider = new CredentialsProvider(secretRepository);
            var googleCredentials = await credentialsProvider.GetCredentialsAsync(stoppingToken);

            var hashingService = await _hashingServiceFactory.CreateAsync(secretRepository, stoppingToken);

            var nonServiceUserCriteria = new NonServiceUserCriteria(_loggerFactory.CreateLogger<NonServiceUserCriteria>());
            var employeeCriterias = new[] { nonServiceUserCriteria };

            var mailboxClient = new MailboxClient(networkId, googleCredentials, _statusLogger, _tasksStatusesCache, _googleConfig, _loggerFactory, _clock);
            var meetingsClient = new CalendarClient(networkId, googleCredentials, _tasksStatusesCache, _googleConfig, _loggerFactory.CreateLogger<CalendarClient>());
            var employeeProfileClient = new UsersClient(network, googleCredentials, _tasksStatusesCache, _googleConfig, employeeCriterias, _loggerFactory.CreateLogger<UsersClient>());

            return new GoogleFacade(network, mailboxClient, meetingsClient, employeeProfileClient, hashingService, _clock, _googleConfig, _loggerFactory.CreateLogger<GoogleFacade>());
        }
    }
}