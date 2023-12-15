using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IChatClient
    {
        Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, CancellationToken stoppingToken = default);
    }

    internal class ChatClient : IChatClient
    {
        private const string TaskCaption = "Synchronizing chat interactions";
        private const string TaskDescription = "Fetching chat metadata from Microsoft API";

        private readonly GraphServiceClient _graphClient;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILogger<ChatClient> _logger;

        public ChatClient(GraphServiceClient graphClient, ITasksStatusesCache tasksStatusesCache, ILogger<ChatClient> logger)
        {
            _graphClient = graphClient;
            _tasksStatusesCache = tasksStatusesCache;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.NetworkId, taskStatus, stoppingToken);
            }

            Task<SingleTaskResult> SingleTaskAsync(string userEmail)
                => GetSingleUserInteractionsAsync(context, stream, userEmail, stoppingToken);

            _logger.LogInformation("Evaluating interactions based on chat for '{timerange}' for {count} users...", context.TimeRange, usersEmails.Count());
            var result = await ParallelSyncTask<string>.RunAsync(usersEmails, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
            _logger.LogInformation("Evaluation of interactions based on chat for '{timerange}' completed", context.TimeRange);

            return result;
        }

        private async Task<SingleTaskResult> GetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, CancellationToken stoppingToken)
        {
            await Task.Yield();
            return new SingleTaskResult(0);
        }
    }
}
