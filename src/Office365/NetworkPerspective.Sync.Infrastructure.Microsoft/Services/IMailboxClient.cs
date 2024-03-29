using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Graph.Users.Item.Messages;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IMailboxClient
    {
        Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IEmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal sealed class MailboxClient : IMailboxClient
    {
        private const string TaskCaption = "Synchronizing email interactions";
        private const string TaskDescription = "Fetching emails metadata from Microsoft API";
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<MailboxClient> _logger;

        public MailboxClient(GraphServiceClient graphClient, ITasksStatusesCache tasksStatusesCache, ILogger<MailboxClient> logger)
        {
            _tasksStatusesCache = tasksStatusesCache;
            _graphClient = graphClient;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IEmailInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.NetworkId, taskStatus, stoppingToken);
            }

            Task<SingleTaskResult> SingleTaskAsync(string userEmail)
                => TryGetSingleUserInteractionsAsync(context, stream, userEmail, interactionFactory, stoppingToken);

            _logger.LogInformation("Evaluating interactions based on mailbox for timerange {timerange} for {count} users...", context.TimeRange, usersEmails.Count());
            var result = await ParallelSyncTask<string>.RunAsync(usersEmails, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
            _logger.LogInformation("Evaluation of interactions based on mailbox for timerange '{timerange}' completed", context.TimeRange);

            return result;
        }

        private async Task<SingleTaskResult> TryGetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, IEmailInteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            try
            {
                return await GetSingleUserInteractionsAsync(context, stream, userEmail, interactionFactory, stoppingToken);
            }
            catch (ODataError ex) when (ex.Error.Code == "MailboxNotEnabledForRESTAPI")
            {
                return SingleTaskResult.Empty;
            }
            catch (ODataError ex) // temp change... hope to find out what edge case is not handled correctly
            {
                throw new System.Exception($"Code: {ex.Error.Code}");
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
                        Filter = filterString
                    };
                    x.Headers.Add("Prefer", "IdType=\"ImmutableId\"");
                }, stoppingToken);

            var pageIterator = PageIterator<Message, MessageCollectionResponse>
                .CreatePageIterator(_graphClient, mailsResponse,
                async mail =>
                {
                    var interactions = interactionFactory.CreateForUser(mail, userEmail);
                    var sentInteractionsCount = await stream.SendAsync(interactions);
                    interactionsCount += sentInteractionsCount;
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