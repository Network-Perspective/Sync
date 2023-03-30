using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IMailboxClient
    {
        Task SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, CancellationToken stoppingToken = default);
    }

    internal sealed class MailboxClient : IMailboxClient
    {
        private const string TaskCaption = "Synchronizing email interactions";
        private const string TaskDescription = "Fetching emails metadata from Microsoft API";
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly IInteractionFactory _interactionFactory;
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<MailboxClient> _logger;

        public MailboxClient(GraphServiceClient graphClient, ITasksStatusesCache tasksStatusesCache, IInteractionFactory interactionFactory, ILogger<MailboxClient> logger)
        {
            _tasksStatusesCache = tasksStatusesCache;
            _interactionFactory = interactionFactory;
            _graphClient = graphClient;
            _logger = logger;
        }

        public async Task SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.NetworkId, taskStatus, stoppingToken);
            }

            Task SingleTaskAsync(string userEmail)
                => GetSingleUserInteractionsAsync(context, stream, userEmail, stoppingToken);

            _logger.LogInformation("Evaluating interactions based on mailbox for timerange {timerange} for {count} users...", context.TimeRange, usersEmails.Count());

            await ParallelTask.RunAsync(usersEmails, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);

            _logger.LogInformation("Evaluation of interactions based on mailbox for timerange '{timerange}' completed", context.TimeRange);
        }

        private async Task GetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, CancellationToken stoppingToken)
        {
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
                }, stoppingToken);

            var pageIterator = PageIterator<Message, MessageCollectionResponse>
                .CreatePageIterator(_graphClient, mailsResponse,
                async mail =>
                {
                    var interactions = _interactionFactory.CreateForUser(mail, userEmail);
                    await stream.SendAsync(interactions);
                    return true;
                },
                request =>
                {
                    return request;
                });

            await pageIterator.IterateAsync(stoppingToken);
        }
    }
}