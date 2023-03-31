using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    internal sealed class MicrosoftFacade : IDataSource
    {
        private readonly IUsersClient _usersClient;
        private readonly IMailboxClient _mailboxClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MicrosoftFacade> _logger;

        public MicrosoftFacade(IUsersClient usersClient, IMailboxClient mailboxClient, ILoggerFactory loggerFactory)
        {
            _usersClient = usersClient;
            _mailboxClient = mailboxClient;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MicrosoftFacade>();
        }

        public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for network '{networkId}'", context.NetworkId);

            var employees = await context.EnsureSetAsync(async () =>
            {
                var users = await _usersClient.GetUsersAsync(context, stoppingToken);
                return EmployeesMapper.ToEmployees(users);
            });

            return employees;
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting hashed employees for network '{networkId}'", context.NetworkId);

            var users = await _usersClient.GetUsersAsync(context, stoppingToken);
            return HashedEmployeesMapper.ToEmployees(users, context.HashFunction);

        }

        public Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Checking if network '{networkId}' is authorized", networkId);

            return Task.FromResult(true);
        }

        public async Task SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for network '{networkId}' for period {timeRange}", context.NetworkId, context.TimeRange);

            var employees = await context.EnsureSetAsync(async () =>
            {
                var users = await _usersClient.GetUsersAsync(context, stoppingToken);
                return EmployeesMapper.ToEmployees(users);
            });

            var emailInteractionfactory = new InteractionFactory(context.HashFunction, employees, _loggerFactory.CreateLogger<InteractionFactory>());

            var usersEmails = employees
                .GetAllInternal()
                .Select(x => x.Id.PrimaryId);

            await _mailboxClient.SyncInteractionsAsync(context, stream, usersEmails, emailInteractionfactory, stoppingToken);

            _logger.LogInformation("Getting interactions for network '{networkId}' completed", context.NetworkId);
        }
    }
}