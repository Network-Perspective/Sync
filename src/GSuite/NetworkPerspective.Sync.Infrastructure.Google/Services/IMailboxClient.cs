using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal interface IMailboxClient
    {
        Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, EmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal sealed class MailboxClient : IMailboxClient
    {
        private static readonly int MinutesInDay = 24 * 60;
        private const string TaskCaption = "Synchronizing email interactions";
        private const string TaskDescription = "Fetching emails metadata from Google API";

        private readonly GoogleConfig _config;
        private readonly ILogger<MailboxClient> _logger;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly IRetryPolicyProvider _retryPolicyProvider;
        private readonly IStatusLogger _statusLogger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IClock _clock;

        public MailboxClient(ITasksStatusesCache tasksStatusesCache, IOptions<GoogleConfig> config, ICredentialsProvider credentialsProvider, IRetryPolicyProvider retryPolicyProvider, IStatusLogger statusLogger, ILoggerFactory loggerFactory, IClock clock)
        {
            _config = config.Value;
            _logger = loggerFactory.CreateLogger<MailboxClient>();
            _tasksStatusesCache = tasksStatusesCache;
            _credentialsProvider = credentialsProvider;
            _retryPolicyProvider = retryPolicyProvider;
            _statusLogger = statusLogger;
            _loggerFactory = loggerFactory;
            _clock = clock;
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, EmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            var maxMessagesCountPerUser = CalculateMaxMessagesCount(context.TimeRange);

            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.ConnectorId, taskStatus, stoppingToken);
            }

            Task<SingleTaskResult> SingleTaskAsync(string userEmail)
            {
                var retryPolicy = _retryPolicyProvider.GetSecretRotationRetryPolicy();
                return retryPolicy.ExecuteAsync(() => GetSingleUserInteractionsAsync(context, stream, userEmail, maxMessagesCountPerUser, interactionFactory, stoppingToken));
            }

            _logger.LogInformation("Evaluating interactions based on mailbox for timerange {timerange} for {count} users...", context.TimeRange, usersEmails.Count());
            var result = await ParallelSyncTask<string>.RunAsync(usersEmails, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
            _logger.LogInformation("Evaluation of interactions based on mailbox for timerange '{timerange}' completed", context.TimeRange);

            return result;
        }

        private async Task<SingleTaskResult> GetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, int maxMessagesCount, EmailInteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            try
            {
                var interactionsCount = 0;
                _logger.LogDebug("Evaluating interactions based on mailbox for user ***...");

                using var gmailService = await InitializeGmailServiceAsync(userEmail, stoppingToken);
                var mailboxTraverser = new MailboxTraverser(userEmail, maxMessagesCount, gmailService, _retryPolicyProvider, _loggerFactory.CreateLogger<MailboxTraverser>());
                var message = await mailboxTraverser.GetNextMessageAsync(stoppingToken);

                var periodStart = context.TimeRange.Start.AddMinutes(-_config.SyncOverlapInMinutes);
                _logger.LogDebug("To not miss any email interactions period start is extended by {minutes}min. As result mailbox interactions are eveluated starting from {start}", _config.SyncOverlapInMinutes, periodStart);

                while (!stoppingToken.IsCancellationRequested && message != null && message?.GetDateTime(_clock) > periodStart)
                {
                    var interactions = interactionFactory.CreateForUser(message, userEmail);
                    var sentInteractionsCount = await stream.SendAsync(interactions);
                    interactionsCount += sentInteractionsCount;
                    message = await mailboxTraverser.GetNextMessageAsync(stoppingToken);
                }

                _logger.LogDebug("Evaluation interactions based on mailbox for user '{user}' completed. Processed {mailsCount} email/s", "***", mailboxTraverser.FetchedMessagesCount);
                _logger.LogTrace("Evaluation interactions based on mailbox for user '{user}' completed. Processed {mailsCount} email/s", userEmail, mailboxTraverser.FetchedMessagesCount);

                return new SingleTaskResult(interactionsCount);
            }
            catch (TooManyMailsPerUserException tmmpuex)
            {
                await _statusLogger.LogWarningAsync($"Skipping mailbox '***' too many messages", stoppingToken);
                _logger.LogWarning("Skipping mailbox '{email}' too many messages", "***");
                _logger.LogTrace("Skipping mailbox '{email}' too many messages", tmmpuex.Email);
                return SingleTaskResult.Empty;
            }
            catch (GoogleApiException gaex) when (IsMailServiceNotEnabledException(gaex))
            {
                _logger.LogWarning("Skipping mailbox '{email}' gmail service not enabled", "***");
                _logger.LogTrace("Skipping mailbox '{email}' gmail service not enabled", userEmail);
                return SingleTaskResult.Empty;
            }
        }

        private int CalculateMaxMessagesCount(TimeRange timeRange)
        {
            var maxMessagesCount = (int)(_config.MaxMessagesPerUserDaily * timeRange.Duration.TotalMinutes / MinutesInDay);

            var result = maxMessagesCount > 0 ? maxMessagesCount : 1;

            _logger.LogDebug("For synchronization period {timerange} max allowed messages is {count}", timeRange, result);

            return result;
        }

        private async Task<GmailService> InitializeGmailServiceAsync(string userEmail, CancellationToken stoppingToken)
        {
            var credentials = await _credentialsProvider.GetForUserAsync(userEmail, stoppingToken);

            return new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
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