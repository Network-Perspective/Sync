using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Mappers;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google
{
    internal sealed class GoogleFacade : IDataSource
    {
        private readonly INetworkService _networkService;
        private readonly ISecretRepository _secretRepository;
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly IMailboxClient _mailboxClient;
        private readonly ICalendarClient _calendarClient;
        private readonly IUsersClient _usersClient;
        private readonly IHashingServiceFactory _hashingServiceFactory;
        private readonly IClock _clock;
        private readonly GoogleConfig _config;
        private readonly ILogger<GoogleFacade> _logger;

        public GoogleFacade(INetworkService networkService,
                            ISecretRepository secretRepository,
                            ICredentialsProvider credentialsProvider,
                            IMailboxClient mailboxClient,
                            ICalendarClient calendarClient,
                            IUsersClient usersClient,
                            IHashingServiceFactory hashingServiceFactory,
                            IClock clock,
                            IOptions<GoogleConfig> config,
                            ILogger<GoogleFacade> logger)
        {
            _networkService = networkService;
            _secretRepository = secretRepository;
            _credentialsProvider = credentialsProvider;
            _mailboxClient = mailboxClient;
            _calendarClient = calendarClient;
            _usersClient = usersClient;
            _hashingServiceFactory = hashingServiceFactory;
            _clock = clock;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<ISet<Interaction>> GetInteractions(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for network '{networkId}' for period {timeRange}", context.NetworkId, context.CurrentRange);

            await InitializeInContext(context, () => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            await InitializeInContext(context, () => _credentialsProvider.GetCredentialsAsync(stoppingToken));
            await InitializeInContext(context, () => _hashingServiceFactory.CreateAsync(_secretRepository, stoppingToken));

            var credentials = context.Get<GoogleCredential>();
            var network = context.Get<Network<GoogleNetworkProperties>>();
            var hashingService = context.Get<IHashingService>();

            var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes));
            var users = await _usersClient.GetUsersAsync(network, context.NetworkConfig, credentials, stoppingToken);

            var employeeCollection = mapper.ToEmployees(users);

            var interactionFactory = new InteractionFactory(hashingService.Hash, employeeCollection, _clock);
            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            var periodStart = context.CurrentRange.Start.AddMinutes(-_config.SyncOverlapInMinutes);
            _logger.LogInformation("To not miss any email interactions period start is extended by {minutes}min. As result mailbox interactions are eveluated starting from {start}", _config.SyncOverlapInMinutes, periodStart);

            await InitializeInContext(context, () => _mailboxClient.GetInteractionsAsync(context.NetworkId, employeeCollection.GetAllInternal(), periodStart, credentials, interactionFactory, stoppingToken));

            var emailInteractions = context.Get<ISet<Interaction>>();
            result.UnionWith(emailInteractions.Where(x => context.CurrentRange.IsInRange(x.Timestamp)));

            var meetingInteractions = await _calendarClient.GetInteractionsAsync(context.NetworkId, employeeCollection.GetAllInternal(), context.CurrentRange, credentials, interactionFactory, stoppingToken);
            result.UnionWith(meetingInteractions);

            _logger.LogInformation("Getting interactions for network '{networkId}' completed", context.NetworkId);

            return result;
        }

        public async Task<EmployeeCollection> GetEmployees(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for network '{networkId}'", context.NetworkId);

            await InitializeInContext(context, () => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            await InitializeInContext(context, () => _credentialsProvider.GetCredentialsAsync(stoppingToken));

            var credentials = context.Get<GoogleCredential>();
            var network = context.Get<Network<GoogleNetworkProperties>>();

            var users = await _usersClient.GetUsersAsync(network, context.NetworkConfig, credentials, stoppingToken);
            var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes));

            return mapper.ToEmployees(users);
        }

        public async Task<EmployeeCollection> GetHashedEmployees(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting hashed employees for network '{networkId}'", context.NetworkId);

            await InitializeInContext(context, () => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            await InitializeInContext(context, () => _credentialsProvider.GetCredentialsAsync(stoppingToken));
            await InitializeInContext(context, () => _hashingServiceFactory.CreateAsync(_secretRepository, stoppingToken));

            var credentials = context.Get<GoogleCredential>();
            var network = context.Get<Network<GoogleNetworkProperties>>();
            var hashingService = context.Get<IHashingService>();

            var users = await _usersClient.GetUsersAsync(network, context.NetworkConfig, credentials, stoppingToken);
            var mapper = new HashedEmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes), hashingService.Hash);

            return mapper.ToEmployees(users);
        }

        public async Task<bool> IsAuthorized(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Checking if network '{networkId}' is authorized", networkId);
                var network = await _networkService.GetAsync<GoogleNetworkProperties>(networkId, stoppingToken);
                var credentials = await _credentialsProvider.GetCredentialsAsync(stoppingToken);

                return await _usersClient.CanGetUsersAsync(network, credentials, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Network '{networkId}' is not authorized", networkId);
                _logger.LogDebug(ex, string.Empty);
                return false;
            }
        }

        private async Task InitializeInContext<T>(SyncContext context, Func<Task<T>> initializer)
        {
            if (!context.Contains<T>())
            {
                _logger.LogDebug($"{typeof(T)} is not initialized yet in the {nameof(SyncContext)}. Initializing {typeof(T)}");
                context.Set(await initializer());
            }
            else
            {
                _logger.LogDebug($"{typeof(T)} is already initialized in {nameof(SyncContext)}");
            }
        }
    }
}