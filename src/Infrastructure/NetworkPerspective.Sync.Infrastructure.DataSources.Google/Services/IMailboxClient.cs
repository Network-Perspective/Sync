﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Exceptions;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal interface IMailboxClient
{
    Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, EmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
}

internal sealed class MailboxClient(IGlobalStatusCache tasksStatusesCache, IOptions<GoogleConfig> config, IImpesonificationCredentialsProvider credentialsProvider, IRetryPolicyProvider retryPolicyProvider, IStatusLogger statusLogger, ILoggerFactory loggerFactory, IClock clock) : IMailboxClient
{
    private static readonly int MinutesInDay = 24 * 60;
    private const string TaskCaption = "Synchronizing email interactions";
    private const string TaskDescription = "Fetching emails metadata from Google API";

    private readonly GoogleConfig _config = config.Value;
    private readonly ILogger<MailboxClient> _logger = loggerFactory.CreateLogger<MailboxClient>();

    public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, EmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
    {
        var maxMessagesCountPerUser = CalculateMaxMessagesCount(context.TimeRange);

        async Task ReportProgressCallbackAsync(double progressRate)
        {
            var taskStatus = SingleTaskStatus.New(TaskCaption, TaskDescription, progressRate);
            await tasksStatusesCache.SetStatusAsync(context.ConnectorId, taskStatus, stoppingToken);
        }

        Task<SingleTaskResult> SingleTaskAsync(string userEmail)
        {
            var retryPolicy = retryPolicyProvider.GetSecretRotationRetryPolicy();
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
            var mailboxTraverser = new MailboxTraverser(userEmail, maxMessagesCount, gmailService, retryPolicyProvider, loggerFactory.CreateLogger<MailboxTraverser>());
            var message = await mailboxTraverser.GetNextMessageAsync(stoppingToken);

            var periodStart = context.TimeRange.Start.AddMinutes(-_config.SyncOverlapInMinutes);
            _logger.LogDebug("To not miss any email interactions period start is extended by {minutes}min. As result mailbox interactions are eveluated starting from {start}", _config.SyncOverlapInMinutes, periodStart);

            while (!stoppingToken.IsCancellationRequested && message != null && message?.GetDateTime(clock) > periodStart)
            {
                var interactions = interactionFactory.CreateForUser(message, userEmail);
                var sentInteractionsCount = await stream.SendAsync(interactions);
                interactionsCount += sentInteractionsCount;
                message = await mailboxTraverser.GetNextMessageAsync(stoppingToken);
            }

            _logger.LogDebug("Evaluation interactions based on mailbox for user '{user}' completed. Processed {mailsCount} email/s", "***", mailboxTraverser.FetchedMessagesCount);

            return new SingleTaskResult(interactionsCount);
        }
        catch (TooManyMailsPerUserException)
        {
            await statusLogger.LogWarningAsync($"Skipping mailbox '***' too many messages", stoppingToken);
            _logger.LogWarning("Skipping mailbox '{email}' too many messages", "***");
            return SingleTaskResult.Empty;
        }
        catch (GoogleApiException gaex) when (IsMailServiceNotEnabledException(gaex))
        {
            _logger.LogWarning("Skipping mailbox '{email}' gmail service not enabled", "***");
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
        var credentials = await credentialsProvider.ImpersonificateAsync(userEmail, stoppingToken);

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