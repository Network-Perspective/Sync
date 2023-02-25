using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Employees;
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
        private readonly IClock _clock;
        private readonly ILoggerFactory _loggerFactory;
        private readonly GoogleConfig _config;
        private readonly ILogger<GoogleFacade> _logger;

        public GoogleFacade(INetworkService networkService,
                            ISecretRepository secretRepository,
                            ICredentialsProvider credentialsProvider,
                            IMailboxClient mailboxClient,
                            ICalendarClient calendarClient,
                            IUsersClient usersClient,
                            IClock clock,
                            IOptions<GoogleConfig> config,
                            ILoggerFactory loggerFactory)
        {
            _networkService = networkService;
            _secretRepository = secretRepository;
            _credentialsProvider = credentialsProvider;
            _mailboxClient = mailboxClient;
            _calendarClient = calendarClient;
            _usersClient = usersClient;
            _clock = clock;
            _loggerFactory = loggerFactory;
            _config = config.Value;
            _logger = loggerFactory.CreateLogger<GoogleFacade>();
        }

        public async Task SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for network '{networkId}' for period {timeRange}", context.NetworkId, context.TimeRange);

            await InitializeInContext(context, () => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            await InitializeInContext(context, () => _credentialsProvider.GetCredentialsAsync(stoppingToken));

            var credentials = context.Get<GoogleCredential>();
            var network = context.Get<Network<GoogleNetworkProperties>>();

            await InitializeInContext(context, () => _usersClient.GetUsersAsync(network, context.NetworkConfig, credentials, stoppingToken));

            var employeeCollection = context.Get<EmployeeCollection>();

            var emailInteractionFactory = new EmailInteractionFactory(context.HashFunction, employeeCollection, _clock, _loggerFactory.CreateLogger<EmailInteractionFactory>());
            var meetingInteractionFactory = new MeetingInteractionFactory(context.HashFunction, employeeCollection, _loggerFactory.CreateLogger<MeetingInteractionFactory>());


            await _mailboxClient.SyncInteractionsAsync(context, stream, employeeCollection.GetAllInternal(), credentials, emailInteractionFactory, stoppingToken);
            await _calendarClient.SyncInteractionsAsync(context, stream, employeeCollection.GetAllInternal(), credentials, meetingInteractionFactory, stoppingToken);

            _logger.LogInformation("Getting interactions for network '{networkId}' completed", context.NetworkId);
        }

        public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for network '{networkId}'", context.NetworkId);

            await InitializeInContext(context, () => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            await InitializeInContext(context, () => _credentialsProvider.GetCredentialsAsync(stoppingToken));

            var credentials = context.Get<GoogleCredential>();
            var network = context.Get<Network<GoogleNetworkProperties>>();

            var users = await _usersClient.GetUsersAsync(network, context.NetworkConfig, credentials, stoppingToken);

            var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes));

            await InitializeInContext(context, () => Task.FromResult(mapper.ToEmployees(users)));

            return context.Get<EmployeeCollection>();
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting hashed employees for network '{networkId}'", context.NetworkId);

            await InitializeInContext(context, () => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            await InitializeInContext(context, () => _credentialsProvider.GetCredentialsAsync(stoppingToken));

            var credentials = context.Get<GoogleCredential>();
            var network = context.Get<Network<GoogleNetworkProperties>>();
            var users = await _usersClient.GetUsersAsync(network, context.NetworkConfig, credentials, stoppingToken);

            var mapper = new HashedEmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes), context.HashFunction);

            return mapper.ToEmployees(users);
        }

        public async Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
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