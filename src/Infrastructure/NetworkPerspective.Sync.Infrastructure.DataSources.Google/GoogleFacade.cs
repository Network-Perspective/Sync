﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Mappers;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google
{
    internal sealed class GoogleFacade : IDataSource
    {
        private readonly IMailboxClient _mailboxClient;
        private readonly ICalendarClient _calendarClient;
        private readonly IUsersClient _usersClient;
        private readonly IUserCalendarTimeZoneReader _userCalendarTimeZoneReader;
        private readonly IClock _clock;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<GoogleFacade> _logger;

        public GoogleFacade(IMailboxClient mailboxClient,
                            ICalendarClient calendarClient,
                            IUsersClient usersClient,
                            IUserCalendarTimeZoneReader userCalendarTimeZoneReader,
                            IClock clock,
                            ILoggerFactory loggerFactory)
        {
            _mailboxClient = mailboxClient;
            _calendarClient = calendarClient;
            _usersClient = usersClient;
            _userCalendarTimeZoneReader = userCalendarTimeZoneReader;
            _clock = clock;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<GoogleFacade>();
        }

        public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for connector '{connectorId}' for period {timeRange}", context.ConnectorId, context.TimeRange);

            var users = await _usersClient.GetUsersAsync(context.ConnectorId, context.GetConnectorProperties<GoogleNetworkProperties>(), context.NetworkConfig, stoppingToken);

            var mapper = new EmployeesMapper(
                new CompanyStructureService(),
                new CustomAttributesService(context.NetworkConfig.CustomAttributes),
                EmployeePropsSource.Empty,
                context.NetworkConfig.EmailFilter);

            var employeesCollection = context.EnsureSet(() => mapper.ToEmployees(users));

            var emailInteractionFactory = new EmailInteractionFactory(context.HashFunction, employeesCollection, _clock, _loggerFactory.CreateLogger<EmailInteractionFactory>());
            var meetingInteractionFactory = new MeetingInteractionFactory(context.HashFunction, employeesCollection, _loggerFactory.CreateLogger<MeetingInteractionFactory>());

            var usersEmails = employeesCollection
                .GetAllInternal()
                .Select(x => x.Id.PrimaryId);

            var resultEmails = await _mailboxClient.SyncInteractionsAsync(context, stream, usersEmails, emailInteractionFactory, stoppingToken);
            var resultCalendar = await _calendarClient.SyncInteractionsAsync(context, stream, usersEmails, meetingInteractionFactory, stoppingToken);

            _logger.LogInformation("Getting interactions for connector '{connectorId}' completed", context.ConnectorId);

            return SyncResult.Combine(resultEmails, resultCalendar);
        }

        public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for connector '{connectorId}'", context.ConnectorId);

            var users = await _usersClient.GetUsersAsync(context.ConnectorId, context.GetConnectorProperties<GoogleNetworkProperties>(), context.NetworkConfig, stoppingToken);

            var timezonesPropsSource = await _userCalendarTimeZoneReader.FetchTimeZoneInformation(users, stoppingToken);

            var mapper = new EmployeesMapper(
                new CompanyStructureService(),
                new CustomAttributesService(context.NetworkConfig.CustomAttributes),
                timezonesPropsSource,
                context.NetworkConfig.EmailFilter
            );

            var employeesCollection = context.EnsureSet(() => mapper.ToEmployees(users));
            return employeesCollection;
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting hashed employees for connector '{connectorId}'", context.ConnectorId);

            var users = await _usersClient.GetUsersAsync(context.ConnectorId, context.GetConnectorProperties<GoogleNetworkProperties>(), context.NetworkConfig, stoppingToken);

            var timezonesPropsSource = await _userCalendarTimeZoneReader.FetchTimeZoneInformation(users, stoppingToken);

            var mapper = new HashedEmployeesMapper(
                new CompanyStructureService(),
                new CustomAttributesService(context.NetworkConfig.CustomAttributes),
                timezonesPropsSource,
                context.HashFunction,
                context.NetworkConfig.EmailFilter
            );

            var employees = mapper.ToEmployees(users);

            return employees;
        }
    }
}