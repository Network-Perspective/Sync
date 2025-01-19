using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Mappers;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google;

internal sealed class GoogleFacade(IMailboxClient mailboxClient,
                    ICalendarClient calendarClient,
                    IUsersClient usersClient,
                    IUserCalendarTimeZoneReader userCalendarTimeZoneReader,
                    IHashingService hashingService,
                    ICredentialsService credentialsService,
                    IClock clock,
                    IEmployeesMapper employeesMapper,
                    IHashedEmployeesMapper hashedEmployeesMapper,
                    ILoggerFactory loggerFactory) : IDataSource
{
    private readonly ILogger<GoogleFacade> _logger = loggerFactory.CreateLogger<GoogleFacade>();

    public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting interactions for connector '{connectorId}' for period {timeRange}", context.ConnectorId, context.TimeRange);

        var connectorProperties = new GoogleConnectorProperties(context.ConnectorProperties);

        var credentials = await credentialsService.ImpersonificateAsync(connectorProperties.AdminEmail, stoppingToken);
        var users = await usersClient.GetUsersAsync(credentials, stoppingToken);

        var timezonesPropsSource = await userCalendarTimeZoneReader.FetchTimeZoneInformation(users, stoppingToken);

        var employeesCollection = context.EnsureSet(() => employeesMapper.ToEmployees(users, timezonesPropsSource));

        var emailInteractionFactory = new EmailInteractionFactory(hashingService.Hash, employeesCollection, clock, loggerFactory.CreateLogger<EmailInteractionFactory>());
        var meetingInteractionFactory = new MeetingInteractionFactory(hashingService.Hash, employeesCollection, loggerFactory.CreateLogger<MeetingInteractionFactory>());

        var usersEmails = employeesCollection
            .GetAllInternal()
            .Select(x => x.Id.PrimaryId);

        var resultEmails = await mailboxClient.SyncInteractionsAsync(context, stream, usersEmails, emailInteractionFactory, stoppingToken);
        var resultCalendar = await calendarClient.SyncInteractionsAsync(context, stream, usersEmails, meetingInteractionFactory, stoppingToken);

        _logger.LogInformation("Getting interactions for connector '{connectorId}' completed", context.ConnectorId);

        return SyncResult.Combine(resultEmails, resultCalendar);
    }

    public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting employees for connector '{connectorId}'", context.ConnectorId);

        var connectorProperties = new GoogleConnectorProperties(context.ConnectorProperties);

        if (connectorProperties.UseUserToken)
            await credentialsService.TryRefreshUserAccessTokenAsync(stoppingToken);

        var credentials = connectorProperties.UseUserToken
            ? await credentialsService.GetUserCredentialsAsync(stoppingToken)
            : await credentialsService.ImpersonificateAsync(connectorProperties.AdminEmail, stoppingToken);
        var users = await usersClient.GetUsersAsync(credentials, stoppingToken);

        var timezonesPropsSource = await userCalendarTimeZoneReader.FetchTimeZoneInformation(users, stoppingToken);

        var employeesCollection = context.EnsureSet(() => employeesMapper.ToEmployees(users, timezonesPropsSource));
        return employeesCollection;
    }

    public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting hashed employees for connector '{connectorId}'", context.ConnectorId);

        var connectorProperties = new GoogleConnectorProperties(context.ConnectorProperties);

        if (connectorProperties.UseUserToken)
            await credentialsService.TryRefreshUserAccessTokenAsync(stoppingToken);

        var credentials = connectorProperties.UseUserToken
            ? await credentialsService.GetUserCredentialsAsync(stoppingToken)
            : await credentialsService.ImpersonificateAsync(connectorProperties.AdminEmail, stoppingToken);

        var users = await usersClient.GetUsersAsync(credentials, stoppingToken);

        var timezonesPropsSource = await userCalendarTimeZoneReader.FetchTimeZoneInformation(users, stoppingToken);

        var employees = hashedEmployeesMapper.ToEmployees(users, timezonesPropsSource);

        return employees;
    }

    public async Task ValidateAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var props = new GoogleConnectorProperties(context.ConnectorProperties);
        await new GoogleConnectorProperties.Validator().ValidateAndThrowAsync(props, stoppingToken);
    }
}