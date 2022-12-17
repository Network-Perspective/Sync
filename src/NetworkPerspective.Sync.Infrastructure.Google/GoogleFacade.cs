using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Mappers;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google
{
    internal sealed class GoogleFacade : IDataSource
    {
        private readonly IMailboxClient _mailboxClient;
        private readonly ICalendarClient _calendarClient;
        private readonly IUsersClient _usersClient;
        private readonly IHashingService _hashingService;
        private readonly IClock _clock;
        private readonly GoogleConfig _config;
        private readonly ILogger<GoogleFacade> _logger;

        public GoogleFacade(IMailboxClient mailboxClient,
                            ICalendarClient calendarClient,
                            IUsersClient usersClient,
                            IHashingService hashingService,
                            IClock clock,
                            IOptions<GoogleConfig> config,
                            ILogger<GoogleFacade> logger)
        {
            _mailboxClient = mailboxClient;
            _calendarClient = calendarClient;
            _usersClient = usersClient;
            _hashingService = hashingService;
            _clock = clock;
            _config = config.Value;
            _logger = logger;
        }

        public async Task InitializeAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            await InitializeInContext(context, async () =>
            {
                var users = await _usersClient.GetUsersAsync(context.NetworkConfig, stoppingToken);
                var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes));
                return mapper.ToEmployees(users);
            });

            await InitializeInContext(context, async () =>
            {
                var employees = context.Get<EmployeeCollection>();

                var interactionFactory = new InteractionFactory(_hashingService.Hash, employees, _clock);

                var periodStart = context.CurrentRange.Start.AddMinutes(-_config.SyncOverlapInMinutes);
                await _mailboxClient.GetInteractionsAsync(context.InteractionsCache, context.NetworkId, employees.GetAllInternal(), periodStart, interactionFactory, stoppingToken);
            });
        }

        public async Task<ISet<Interaction>> GetInteractions(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for network '{networkId}' for period {timeRange}", context.NetworkId, context.CurrentRange);

            var hashingService = context.Get<IHashingService>();

            var users = context.Get<IEnumerable<User>>();
            var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes));
            var employeeCollection = mapper.ToEmployees(users);

            var interactionFactory = new InteractionFactory(hashingService.Hash, employeeCollection, _clock);
            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            var periodStart = context.CurrentRange.Start.AddMinutes(-_config.SyncOverlapInMinutes);
            _logger.LogInformation("To not miss any email interactions period start is extended by {minutes}min. As result mailbox interactions are eveluated starting from {start}", _config.SyncOverlapInMinutes, periodStart);

            await _mailboxClient.GetInteractionsAsync(context.InteractionsCache, employeeCollection.GetAllInternal(), periodStart, interactionFactory, stoppingToken);

            var mailboxInteractions = await context.InteractionsCache.PullInteractionsAsync(context.CurrentRange.Start.Date, stoppingToken);
            result.UnionWith(mailboxInteractions);

            var usersEmails = employeeCollection.GetAllInternal().Select(x => x.Id.PrimaryId);

            var meetingInteractions = await _calendarClient.GetInteractionsAsync(usersEmails, context.CurrentRange, interactionFactory, stoppingToken);
            result.UnionWith(meetingInteractions);

            _logger.LogInformation("Getting interactions for network '{networkId}' completed", context.NetworkId);

            return result;
        }

        public Task<EmployeeCollection> GetEmployees(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for network '{networkId}'", context.NetworkId);

            var users = context.Get<IEnumerable<User>>();
            var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes));
            return Task.FromResult(mapper.ToEmployees(users));
        }

        public Task<EmployeeCollection> GetHashedEmployees(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting hashed employees for network '{networkId}'", context.NetworkId);

            var users = context.Get<IEnumerable<User>>();

            var mapper = new HashedEmployeesMapper(new CompanyStructureService(), new CustomAttributesService(context.NetworkConfig.CustomAttributes), _hashingService.Hash);
            return Task.FromResult(mapper.ToEmployees(users));
        }

        public async Task<bool> IsAuthorized(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Checking if network '{networkId}' is authorized", networkId);

                return await _usersClient.CanGetUsersAsync(_googleCredential, stoppingToken);
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