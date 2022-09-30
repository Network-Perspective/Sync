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

            var credentialsProvider = new CredentialsProvider(secretRepository);
            var emailClient = new EmailClient(_statusLogger, _tasksStatusesCache, _googleConfig, _loggerFactory, _clock);
            var meetingsClient = new MeetingClient(_tasksStatusesCache, _googleConfig, _loggerFactory.CreateLogger<MeetingClient>());
            var employeeCriterias = new[] { new NonServiceUserCriteria(_loggerFactory.CreateLogger<NonServiceUserCriteria>()) };
            var employeeProfileClient = new UsersClient(_tasksStatusesCache, _googleConfig, employeeCriterias, _loggerFactory.CreateLogger<UsersClient>());

            return new GoogleFacade(_networkService, secretRepository, credentialsProvider, emailClient, meetingsClient, employeeProfileClient, _hashingServiceFactory, _clock, _googleConfig, _loggerFactory.CreateLogger<GoogleFacade>());
        }
    }
}