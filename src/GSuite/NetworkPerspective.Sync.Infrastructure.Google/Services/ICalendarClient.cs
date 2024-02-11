using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Meetings;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal interface ICalendarClient
    {
        public Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, MeetingInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal sealed class CalendarClient : ICalendarClient
    {
        private const string TaskCaption = "Synchronizing callendar interactions";
        private const string TaskDescription = "Fetching callendar metadata from Google API";

        private readonly GoogleConfig _config;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILogger<CalendarClient> _logger;
        private readonly IThrottlingRetryHandler _retryHandler;
        private readonly ICredentialsProvider _credentialsProvider;

        public CalendarClient(ITasksStatusesCache tasksStatusesCache, IOptions<GoogleConfig> config, IThrottlingRetryHandler retryHandler, ICredentialsProvider credentialsProvider, ILogger<CalendarClient> logger)
        {
            _config = config.Value;
            _tasksStatusesCache = tasksStatusesCache;
            _retryHandler = retryHandler;
            _credentialsProvider = credentialsProvider;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, MeetingInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.NetworkId, taskStatus, stoppingToken);
            }

            Task<SingleTaskResult> SingleTaskAsync(string userEmail)
                => GetSingleUserInteractionsAsync(context, stream, userEmail, interactionFactory, stoppingToken);

            _logger.LogInformation("Evaluating interactions based on callendar for '{timerange}' for {count} users...", context.TimeRange, usersEmails.Count());
            var result = await ParallelSyncTask<string>.RunAsync(usersEmails, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
            _logger.LogInformation("Evaluation of interactions based on callendar for '{timerange}' completed", context.TimeRange);

            return result;
        }

        private async Task<SingleTaskResult> GetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, MeetingInteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogDebug("Evaluating interactions based on callendar for user '{email}' for period {timeRange}...", "***", context.TimeRange);
                _logger.LogTrace("Evaluating interactions based on callendar for user '{email}' for period {timeRange}...", userEmail, context.TimeRange);

                int interactionsCount = 0;

                var googleCredentials = await _credentialsProvider.GetCredentialsAsync(stoppingToken);

                var userCredentials = googleCredentials
                    .CreateWithUser(userEmail)
                    .UnderlyingCredential as ServiceAccountCredential;

                var calendarService = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = userCredentials,
                    ApplicationName = _config.ApplicationName
                });

                var request = calendarService.Events.List(userEmail);
                request.TimeMinDateTimeOffset = context.TimeRange.Start;
                request.TimeMaxDateTimeOffset = context.TimeRange.End;
                request.SingleEvents = true;

                var response = await _retryHandler.ExecuteAsync(request.ExecuteAsync, _logger, stoppingToken);

                _logger.LogDebug("Found '{count}' events in callendar for user '{email}' for period {timeRange}", response.Items.Count, "***", context.TimeRange);
                _logger.LogTrace("Found '{count}' events in callendar for user '{email}' for period {timeRange}", response.Items.Count, userEmail, context.TimeRange);

                foreach (var meeting in response.Items)
                {
                    var recurrence = await GetRecurrenceAsync(calendarService, userEmail, meeting.RecurringEventId, stoppingToken);
                    var interactions = interactionFactory.CreateForUser(meeting, userEmail, recurrence);
                    var sentInteractionsCount = await stream.SendAsync(interactions);
                    interactionsCount += sentInteractionsCount;
                }

                _logger.LogDebug("Evaluation of interactions based on callendar for user '{email}' completed", "***");
                _logger.LogTrace("Evaluation of interactions based on callendar for user '{email}' completed", userEmail);
                return new SingleTaskResult(interactionsCount);
            }
            catch (GoogleApiException gaex) when (IndicatesIsNotACalendarUser(gaex))
            {
                _logger.LogDebug("The user '{email}' is not a calendar user. Skipping meetings interactions for given user", "***");
                _logger.LogTrace("The user '{email}' is not a calendar user. Skipping meetings interactions for given user", userEmail);
                return SingleTaskResult.Empty;
            }
        }

        private async Task<RecurrenceType?> GetRecurrenceAsync(CalendarService calendarService, string userEmail, string recurrenceEventId, CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(recurrenceEventId))
                return null;

            try
            {
                var request = calendarService.Events.Get(userEmail, recurrenceEventId);

                var response = await _retryHandler.ExecuteAsync(request.ExecuteAsync, _logger, stoppingToken);
                return response.GetRecurrence();

            }
            catch (GoogleApiException ex) when (IndicatesNotFound(ex))
            {
                _logger.LogWarning(ex, "Unable to evaluate interaction's recurrence type - it might be caused by event changed. Assigning default value (null)");
                return null;
            }

        }

        private static bool IndicatesIsNotACalendarUser(GoogleApiException ex)
        {
            if (ex.Error.Errors.Count != 1)
                return false;

            var notCalendarUserError = ex.Error.Errors.SingleOrDefault(x => x.Domain == "calendar" && x.Reason == "notACalendarUser");

            return notCalendarUserError is not null;
        }

        private static bool IndicatesNotFound(GoogleApiException ex)
            => ex.HttpStatusCode == HttpStatusCode.NotFound;
    }
}