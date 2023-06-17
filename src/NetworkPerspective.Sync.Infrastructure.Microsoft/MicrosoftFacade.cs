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
        private readonly ICalendarClient _calendarClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MicrosoftFacade> _logger;

        public MicrosoftFacade(IUsersClient usersClient, IMailboxClient mailboxClient, ICalendarClient calendarClient, ILoggerFactory loggerFactory)
        {
            _usersClient = usersClient;
            _mailboxClient = mailboxClient;
            _calendarClient = calendarClient;
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

        public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for network '{networkId}' for period {timeRange}", context.NetworkId, context.TimeRange);

            var employees = await context.EnsureSetAsync(async () =>
            {
                var users = await _usersClient.GetUsersAsync(context, stoppingToken);
                return EmployeesMapper.ToEmployees(users);
            });

            var emailInteractionfactory = new EmailInteractionFactory(context.HashFunction, employees, _loggerFactory.CreateLogger<EmailInteractionFactory>());
            var meetingInteractionfactory = new MeetingInteractionFactory(context.HashFunction, employees, _loggerFactory.CreateLogger<MeetingInteractionFactory>());

            var usersEmails = employees
                .GetAllInternal()
                .Select(x => x.Id.PrimaryId);

            var resultEmails = await _mailboxClient.SyncInteractionsAsync(context, stream, usersEmails, emailInteractionfactory, stoppingToken);
            var resultCalendar = await _calendarClient.SyncInteractionsAsync(context, stream, usersEmails, meetingInteractionfactory, stoppingToken);

            _logger.LogInformation("Getting interactions for network '{networkId}' completed", context.NetworkId);

            return SyncResult.Combine(resultEmails, resultCalendar);
        }
    }
}