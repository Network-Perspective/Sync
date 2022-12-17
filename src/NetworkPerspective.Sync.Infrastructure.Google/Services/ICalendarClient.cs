﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Aggregation;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Meetings;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal interface ICalendarClient
    {
        public Task<ISet<Interaction>> GetInteractionsAsync(IEnumerable<string> usersEmails, TimeRange timeRange, InteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal sealed class CalendarClient : ICalendarClient
    {
        private const string TaskCaption = "Synchronizing callendar interactions";
        private const string TaskDescription = "Fetching callendar metadata from Google API";

        private readonly Guid _networkId;
        private readonly GoogleCredential _googleCredential;
        private readonly GoogleConfig _config;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILogger<CalendarClient> _logger;
        private readonly IThrottlingRetryHandler _retryHandler = new ThrottlingRetryHandler();

        public CalendarClient(Guid networkId, GoogleCredential googleCredential, ITasksStatusesCache tasksStatusesCache, IOptions<GoogleConfig> config, ILogger<CalendarClient> logger)
        {
            _config = config.Value;
            _networkId = networkId;
            _googleCredential = googleCredential;
            _tasksStatusesCache = tasksStatusesCache;
            _logger = logger;
        }

        public async Task<ISet<Interaction>> GetInteractionsAsync(IEnumerable<string> usersEmails, TimeRange timeRange, InteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(_networkId, taskStatus, stoppingToken);
            }

            Task<ISet<Interaction>> SingleTaskAsync(string userEmail)
                => GetSingleUserInteractionsAsync(userEmail, timeRange, interactionFactory, stoppingToken);

            _logger.LogInformation("Evaluating interactions based on callendar for '{timerange}' for {count} users...", timeRange, usersEmails.Count());

            var result = await ParallelFetcher.FetchAsync(usersEmails, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);

            _logger.LogInformation("Evaluation of interactions based on callendar for '{timerange}' completed", timeRange);

            return result;
        }

        private async Task<ISet<Interaction>> GetSingleUserInteractionsAsync(string userEmail, TimeRange timeRange, InteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogDebug("Evaluating interactions based on callendar for period {timeRange} for user ***...", timeRange);

                var userCredentials = _googleCredential
                    .CreateWithUser(userEmail)
                    .UnderlyingCredential as ServiceAccountCredential;

                var calendarService = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = userCredentials,
                    ApplicationName = _config.ApplicationName
                });

                var request = calendarService.Events.List(userEmail);
                request.TimeMin = timeRange.Start;
                request.TimeMax = timeRange.End;
                request.SingleEvents = true;

                var response = await _retryHandler.ExecuteAsync(request.ExecuteAsync, _logger, stoppingToken);

                _logger.LogDebug("Found '{count}' events in callendar for user '{email}' for period {timeRange}", response.Items.Count, "***", timeRange);
                _logger.LogTrace("Found '{count}' events in callendar for user '{email}' for period {timeRange}", response.Items.Count, userEmail, timeRange);

                var result = new HashSet<Interaction>(new InteractionEqualityComparer());

                var actionsAggregator = new ActionsAggregator(userEmail);
                foreach (var meeting in response.Items)
                {
                    var recurrence = await GetRecurrenceAsync(calendarService, userEmail, meeting.RecurringEventId, stoppingToken);
                    actionsAggregator.Add(meeting.GetStart());
                    result.UnionWith(interactionFactory.CreateFromMeeting(meeting, recurrence));
                }

                _logger.LogDebug("Evaluation of interactions based on callendar for user '{email}' completed. Found {count} interactions", "***", result.Count);
                _logger.LogTrace("Evaluation of interactions based on callendar for user '{email}' completed. Found {count} interactions", userEmail, result.Count);
                _logger.LogTrace(new DefaultActionsAggregatorPrinter().Print(actionsAggregator));

                return result;
            }
            catch (GoogleApiException gaex) when (IndicatesIsNotACalendarUser(gaex))
            {
                _logger.LogDebug("The user is not a calendar user. Skipping meetings interactions for given user");
                return ImmutableHashSet<Interaction>.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to evaluate interactions based on callendar for given user. Please see inner exception\n");
                return ImmutableHashSet<Interaction>.Empty;
            }
        }

        private async Task<RecurrenceType?> GetRecurrenceAsync(CalendarService calendarService, string userEmail, string recurrenceEventId, CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(recurrenceEventId))
                return null;

            var request = calendarService.Events.Get(userEmail, recurrenceEventId);
            var response = await _retryHandler.ExecuteAsync(request.ExecuteAsync, _logger, stoppingToken);

            return response.GetRecurrence();
        }

        private static bool IndicatesIsNotACalendarUser(GoogleApiException ex)
        {
            if (ex.Error.Errors.Count != 1)
                return false;

            var notCalendarUserError = ex.Error.Errors.SingleOrDefault(x => x.Domain == "calendar" && x.Reason == "notACalendarUser");

            return notCalendarUserError is not null;
        }
    }
}