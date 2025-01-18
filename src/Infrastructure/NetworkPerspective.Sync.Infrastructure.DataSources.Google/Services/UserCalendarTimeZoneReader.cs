using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

public interface IUserCalendarTimeZoneReader
{
    Task<IEmployeePropsSource> FetchTimeZoneInformation(IEnumerable<User> users, CancellationToken stoppingToken = default);
}

internal class UserCalendarTimeZoneReader(IOptions<GoogleConfig> config, IImpesonificationCredentialsProvider credentialsProvider, IRetryPolicyProvider retryPolicyProvider, ILogger<UserCalendarTimeZoneReader> logger) : IUserCalendarTimeZoneReader
{
    private readonly GoogleConfig _config = config.Value;

    public async Task<IEmployeePropsSource> FetchTimeZoneInformation(IEnumerable<User> users, CancellationToken stoppingToken = default)
    {
        var result = new EmployeePropsSource();

        foreach (User user in users)
        {
            if (stoppingToken.IsCancellationRequested) break;
            try
            {
                var retryPolicy = retryPolicyProvider.GetSecretRotationRetryPolicy();
                var timezone = await retryPolicy.ExecuteAsync(() => ReadUserTimeZoneAsync(user.PrimaryEmail, stoppingToken));
                if (timezone != null)
                    result.AddPropForUser(user.PrimaryEmail, Employee.PropKeyTimezone, timezone);
            }
            catch (GoogleApiException apiException)
                when (apiException.Error?.Errors?.Any(e => e.Reason == "notACalendarUser") == true)
            {
                // ignore users without calendar
            }
            catch (Exception e)
            {
                logger.LogInformation("Failed to read timezone: {exception}", e);
            }
        }

        return result;
    }

    private async Task<string> ReadUserTimeZoneAsync(string userEmail, CancellationToken stoppingToken)
    {
        var credentials = await credentialsProvider.ImpersonificateAsync(userEmail, stoppingToken);

        var calendarService = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = _config.ApplicationName
        });

        // Fetch the list of user's calendars.
        var calendarList = await calendarService.CalendarList.List().ExecuteAsync(stoppingToken);

        // find the first calendar with a timezone set
        var timezone = calendarList?.Items?
            .Where(c => c.AccessRole == "owner" && c.TimeZone != null)
            .GroupBy(c => c.TimeZone)
            .MaxBy(g => g.Count())?
            .Key;

        return timezone;
    }
}