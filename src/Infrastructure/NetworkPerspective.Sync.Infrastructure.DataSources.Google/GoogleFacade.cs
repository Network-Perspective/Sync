using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                    ICredentialsProvider credentialsProvider,
                    IClock clock,
                    ILoggerFactory loggerFactory) : IDataSource
{
    private readonly ILogger<GoogleFacade> _logger = loggerFactory.CreateLogger<GoogleFacade>();

    public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting interactions for connector '{connectorId}' for period {timeRange}", context.ConnectorId, context.TimeRange);

        var connectorProperties = context.GetConnectorProperties<GoogleNetworkProperties>();

        var credentials = await credentialsProvider.ImpersonificateAsync(connectorProperties.AdminEmail, stoppingToken);
        var users = await usersClient.GetUsersAsync(context.ConnectorId, credentials, context.NetworkConfig.EmailFilter, stoppingToken);

        var mapper = new EmployeesMapper(
            new CompanyStructureService(),
            new CustomAttributesService(context.NetworkConfig.CustomAttributes),
            EmployeePropsSource.Empty,
            context.NetworkConfig.EmailFilter);

        var employeesCollection = context.EnsureSet(() => mapper.ToEmployees(users));

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

        var connectorProperties = context.GetConnectorProperties<GoogleNetworkProperties>();

        var credentials = await credentialsProvider.ImpersonificateAsync(connectorProperties.AdminEmail, stoppingToken);
        var users = await usersClient.GetUsersAsync(context.ConnectorId, credentials, context.NetworkConfig.EmailFilter, stoppingToken);

        var timezonesPropsSource = await userCalendarTimeZoneReader.FetchTimeZoneInformation(users, stoppingToken);

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

        var connectorProperties = context.GetConnectorProperties<GoogleNetworkProperties>();

        var credentials = await credentialsProvider.ImpersonificateAsync(connectorProperties.AdminEmail, stoppingToken);
        var users = await usersClient.GetUsersAsync(context.ConnectorId, credentials, context.NetworkConfig.EmailFilter, stoppingToken);

        var timezonesPropsSource = await userCalendarTimeZoneReader.FetchTimeZoneInformation(users, stoppingToken);

        var mapper = new HashedEmployeesMapper(
            new CompanyStructureService(),
            new CustomAttributesService(context.NetworkConfig.CustomAttributes),
            timezonesPropsSource,
            hashingService.Hash,
            context.NetworkConfig.EmailFilter
        );

        var employees = mapper.ToEmployees(users);

        return employees;
    }
}