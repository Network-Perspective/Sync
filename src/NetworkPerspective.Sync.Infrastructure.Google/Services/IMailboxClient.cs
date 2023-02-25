using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Aggregation;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal interface IMailboxClient
    {
        Task SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<Employee> userEmails, GoogleCredential credentials, EmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal sealed class MailboxClient : IMailboxClient
    {
        private static readonly int MinutesInDay = 24 * 60;
        private const string TaskCaption = "Synchronizing email interactions";
        private const string TaskDescription = "Fetching emails metadata from Google API";

        private readonly GoogleConfig _config;
        private readonly ILogger<MailboxClient> _logger;
        private readonly IThrottlingRetryHandler _retryHandler = new ThrottlingRetryHandler();
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IClock _clock;

        public MailboxClient(ITasksStatusesCache tasksStatusesCache, IOptions<GoogleConfig> config, ILoggerFactory loggerFactory, IClock clock)
        {
            _config = config.Value;
            _logger = loggerFactory.CreateLogger<MailboxClient>();
            _tasksStatusesCache = tasksStatusesCache;
            _loggerFactory = loggerFactory;
            _clock = clock;
        }

        public async Task SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<Employee> users, GoogleCredential credentials, EmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            var maxMessagesCountPerUser = CalculateMaxMessagesCount(context.TimeRange);

            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.NetworkId, taskStatus, stoppingToken);
            }

            Task SingleTaskAsync(string userEmail)
                => GetSingleUserInteractionsAsync(context, stream, userEmail, maxMessagesCountPerUser, credentials, interactionFactory, stoppingToken);

            _logger.LogInformation("Evaluating interactions based on mailbox for timerange {timerange} for {count} users...", context.TimeRange, users.Count());

            var userEmails = users.Select(x => x.Id.PrimaryId);
            await ParallelTask.RunAsync(userEmails, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);

            _logger.LogInformation("Evaluation of interactions based on mailbox for timerange '{timerange}' completed", context.TimeRange);
        }

        private async Task GetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, int maxMessagesCount, GoogleCredential credentials, EmailInteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogDebug("Evaluating interactions based on mailbox for user ***...");

                using var gmailService = InitializeGmailService(userEmail, credentials);
                var actionsAggregator = new ActionsAggregator(userEmail);
                var mailboxTraverser = new MailboxTraverser(userEmail, maxMessagesCount, gmailService, _retryHandler, _loggerFactory.CreateLogger<MailboxTraverser>());
                var message = await mailboxTraverser.GetNextMessageAsync(stoppingToken);

                var periodStart = context.TimeRange.Start.AddMinutes(-_config.SyncOverlapInMinutes);
                _logger.LogDebug("To not miss any email interactions period start is extended by {minutes}min. As result mailbox interactions are eveluated starting from {start}", _config.SyncOverlapInMinutes, periodStart);


                while (!stoppingToken.IsCancellationRequested && message != null && message?.GetDateTime(_clock) > periodStart)
                {
                    actionsAggregator.Add(message.GetDateTime(_clock));
                    var interactions = interactionFactory.CreateForUser(message, userEmail);
                    await stream.SendAsync(interactions);
                    message = await mailboxTraverser.GetNextMessageAsync(stoppingToken);
                }

                _logger.LogDebug("Evaluation interactions based on mailbox for user '{user}' completed. Processed {mailsCount} email/s", "***", mailboxTraverser.FetchedMessagesCount);
                _logger.LogTrace("Evaluation interactions based on mailbox for user '{user}' completed. Processed {mailsCount} email/s", userEmail, mailboxTraverser.FetchedMessagesCount);

                _logger.LogTrace(new DefaultActionsAggregatorPrinter().Print(actionsAggregator));
            }
            catch (TooManyMailsPerUserException tmmpuex)
            {
                await context.StatusLogger.LogWarningAsync($"Skipping mailbox '***' too many messages", stoppingToken);
                _logger.LogWarning("Skipping mailbox '{email}' too many messages", "***");
                _logger.LogTrace("Skipping mailbox '{email}' too many messages", tmmpuex.Email);
            }
            catch (GoogleApiException gaex) when (IsMailServiceNotEnabledException(gaex))
            {
                _logger.LogWarning("Skipping mailbox '{email}' gmail service not enabled", "***");
                _logger.LogTrace("Skipping mailbox '{email}' gmail service not enabled", userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to evaluate interactions based on mailbox for given user. Please see inner exception\n");
            }
        }

        private int CalculateMaxMessagesCount(TimeRange timeRange)
        {
            var maxMessagesCount = (int)(_config.MaxMessagesPerUserDaily * timeRange.Duration.TotalMinutes / MinutesInDay);

            var result = maxMessagesCount > 0 ? maxMessagesCount : 1;

            _logger.LogDebug("For synchronization period {timerange} max allowed messages is {count}", timeRange, result);

            return result;
        }

        private GmailService InitializeGmailService(string userEmail, GoogleCredential credentials)
        {
            var userCredentials = credentials
                .CreateWithUser(userEmail)
                .UnderlyingCredential as ServiceAccountCredential;

            return new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredentials,
                ApplicationName = _config.ApplicationName
            });
        }

        private static bool IsMailServiceNotEnabledException(GoogleApiException exception)
        {
            const string exceptionDomain = "global";
            const string exceptionReason = "failedPrecondition";
            const string exceptionMessage = "Mail service not enabled";
            return exception.Error.Errors.Any(x => x.Domain == exceptionDomain && x.Reason == exceptionReason && x.Message == exceptionMessage);
        }
    }
}