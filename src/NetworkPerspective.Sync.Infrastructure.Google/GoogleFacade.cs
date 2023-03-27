using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Mappers;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google
{
    internal sealed class GoogleFacade : IDataSource
    {
        private readonly INetworkService _networkService;
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly IMailboxClient _mailboxClient;
        private readonly ICalendarClient _calendarClient;
        private readonly IUsersClient _usersClient;
        private readonly IClock _clock;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<GoogleFacade> _logger;

        public GoogleFacade(INetworkService networkService,
                            ICredentialsProvider credentialsProvider,
                            IMailboxClient mailboxClient,
                            ICalendarClient calendarClient,
                            IUsersClient usersClient,
                            IClock clock,
                            ILoggerFactory loggerFactory)
        {
            _networkService = networkService;
            _credentialsProvider = credentialsProvider;
            _mailboxClient = mailboxClient;
            _calendarClient = calendarClient;
            _usersClient = usersClient;
            _clock = clock;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<GoogleFacade>();
        }

        public async Task SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for network '{networkId}' for period {timeRange}", context.NetworkId, context.TimeRange);

            var network = await context.EnsureSetAsync(() => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            var credentials = await context.EnsureSetAsync(() => _credentialsProvider.GetCredentialsAsync(stoppingToken));

            var users = await _usersClient.GetUsersAsync(network, context.NetworkConfig, credentials, stoppingToken);

            var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes));

            var employeesCollection = context.EnsureSet(() => mapper.ToEmployees(users));

            var emailInteractionFactory = new EmailInteractionFactory(context.HashFunction, employeesCollection, _clock, _loggerFactory.CreateLogger<EmailInteractionFactory>());
            var meetingInteractionFactory = new MeetingInteractionFactory(context.HashFunction, employeesCollection, _loggerFactory.CreateLogger<MeetingInteractionFactory>());

            await _mailboxClient.SyncInteractionsAsync(context, stream, employeesCollection.GetAllInternal(), credentials, emailInteractionFactory, stoppingToken);
            await _calendarClient.SyncInteractionsAsync(context, stream, employeesCollection.GetAllInternal(), credentials, meetingInteractionFactory, stoppingToken);

            _logger.LogInformation("Getting interactions for network '{networkId}' completed", context.NetworkId);
        }

        public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for network '{networkId}'", context.NetworkId);

            var network = await context.EnsureSetAsync(() => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            var credentials = await context.EnsureSetAsync(() => _credentialsProvider.GetCredentialsAsync(stoppingToken));

            var users = await _usersClient.GetUsersAsync(network, context.NetworkConfig, credentials, stoppingToken);

            var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes));

            var employeesCollection = context.EnsureSet(() => mapper.ToEmployees(users));
            return employeesCollection;
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting hashed employees for network '{networkId}'", context.NetworkId);

            var network = await context.EnsureSetAsync(() => _networkService.GetAsync<GoogleNetworkProperties>(context.NetworkId, stoppingToken));
            var credentials = await context.EnsureSetAsync(() => _credentialsProvider.GetCredentialsAsync(stoppingToken));

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
    }
}