﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Graph.Users.Item.Messages;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services
{
    internal interface IMailboxClient
    {
        Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IEmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal sealed class MailboxClient : IMailboxClient
    {
        private const string TaskCaption = "Synchronizing email interactions";
        private const string TaskDescription = "Fetching emails metadata from Microsoft API";
        private readonly IGlobalStatusCache _tasksStatusesCache;
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<MailboxClient> _logger;

        public MailboxClient(GraphServiceClient graphClient, IGlobalStatusCache tasksStatusesCache, ILogger<MailboxClient> logger)
        {
            _tasksStatusesCache = tasksStatusesCache;
            _graphClient = graphClient;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IEmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = SingleTaskStatus.New(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.ConnectorId, taskStatus, stoppingToken);
            }

            Task<SingleTaskResult> SingleTaskAsync(string userEmail)
                => TryGetSingleUserInteractionsAsync(context, stream, userEmail, interactionFactory, stoppingToken);

            _logger.LogInformation("Evaluating interactions based on mailbox for timerange {timerange} for {count} users...", context.TimeRange, usersEmails.Count());
            var result = await ParallelSyncTask<string>.RunSequentialAsync(usersEmails, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
            _logger.LogInformation("Evaluation of interactions based on mailbox for timerange '{timerange}' completed", context.TimeRange);

            return result;
        }

        private async Task<SingleTaskResult> TryGetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, IEmailInteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            try
            {
                return await GetSingleUserInteractionsAsync(context, stream, userEmail, interactionFactory, stoppingToken);
            }
            catch (ODataError ex) when (ex.Error.Code == Consts.ErrorCodes.MailboxInactive)
            {
                return SingleTaskResult.Empty;
            }
        }

        private async Task<SingleTaskResult> GetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, IEmailInteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            var interactionsCount = 0;
            var filterString = $"receivedDateTime ge {context.TimeRange.Start:s}Z and receivedDateTime lt {context.TimeRange.End:s}Z";

            var mailsResponse = await _graphClient
            .Users[userEmail]
            .Messages
            .GetAsync(x =>
            {
                x.QueryParameters = new MessagesRequestBuilder.MessagesRequestBuilderGetQueryParameters
                {
                    Filter = filterString,
                    Top = 500
                };
                x.Headers.Add("Prefer", "IdType=\"ImmutableId\"");
            }, stoppingToken);

            var pageIterator = PageIterator<Message, MessageCollectionResponse>
                .CreatePageIterator(_graphClient, mailsResponse,
                async mail =>
                {
                    try
                    {
                        var interactions = interactionFactory.CreateForUser(mail, userEmail);
                        var sentInteractionsCount = await stream.SendAsync(interactions);
                        interactionsCount += sentInteractionsCount;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing email page");
                    }

                    return true;
                },
                request =>
                {
                    return request;
                });

            await pageIterator.IterateAsync(stoppingToken);
            return new SingleTaskResult(interactionsCount);
        }
    }
}