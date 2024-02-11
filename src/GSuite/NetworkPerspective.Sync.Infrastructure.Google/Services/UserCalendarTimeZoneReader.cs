using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services;

public interface IUserCalendarTimeZoneReader
{
    Task<IEmployeePropsSource> FetchTimeZoneInformation(IEnumerable<User> users, CancellationToken stoppingToken = default);
}

public class UserCalendarTimeZoneReader : IUserCalendarTimeZoneReader
{
    private readonly ICredentialsProvider _credentialsProvider;
    private readonly ILogger<UserCalendarTimeZoneReader> _logger;
    private readonly GoogleConfig _config;

    public UserCalendarTimeZoneReader(IOptions<GoogleConfig> config, ICredentialsProvider credentialsProvider, ILogger<UserCalendarTimeZoneReader> logger)
    {
        _credentialsProvider = credentialsProvider;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<IEmployeePropsSource> FetchTimeZoneInformation(IEnumerable<User> users, CancellationToken stoppingToken = default)
    {
        var result = new EmployeePropsSource();

        foreach (User user in users)
        {
            if (stoppingToken.IsCancellationRequested) break;
            try
            {
                var timezone = await ReadUserTimeZoneAsync(user.PrimaryEmail, stoppingToken);
                if (timezone != null)
                    result.AddPropForUser(user.PrimaryEmail, "Timezone", timezone);
            }
            catch (GoogleApiException apiException)
                when (apiException.Error?.Errors?.Any(e => e.Reason == "notACalendarUser") == true)
            {
                // ignore users without calendar
            }
            catch (Exception e)
            {
                _logger.LogInformation("Failed to read timezone: {exception}", e);
            }
        }

        return result;
    }

    private async Task<string> ReadUserTimeZoneAsync(string userEmail, CancellationToken stoppingToken)
    {
        var googleCredentials = await _credentialsProvider.GetCredentialsAsync(stoppingToken);

        var userCredentials = googleCredentials
            .CreateWithUser(userEmail)
            .UnderlyingCredential as ServiceAccountCredential;

        var calendarService = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = userCredentials,
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