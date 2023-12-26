using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Calendar.CalendarView;
using Microsoft.Graph.Users.Item.Calendar.Events.Item;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface ICalendarClient
    {
        Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IMeetingInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal class CalendarClient : ICalendarClient
    {
        private const string TaskCaption = "Synchronizing callendar interactions";
        private const string TaskDescription = "Fetching callendar metadata from Microsoft API";

        private readonly GraphServiceClient _graphClient;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILogger<CalendarClient> _logger;

        public CalendarClient(GraphServiceClient graphClient, ITasksStatusesCache tasksStatusesCache, ILogger<CalendarClient> logger)
        {
            _graphClient = graphClient;
            _tasksStatusesCache = tasksStatusesCache;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IMeetingInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
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

        private async Task<SingleTaskResult> GetSingleUserInteractionsAsync(SyncContext context, IInteractionsStream stream, string userEmail, IMeetingInteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            var interactionsCount = 0;
            var mailsResponse = await _graphClient
                .Users[userEmail]
                .Calendar
                .CalendarView
                .GetAsync(x =>
                {
                    x.QueryParameters = new CalendarViewRequestBuilder.CalendarViewRequestBuilderGetQueryParameters()
                    {
                        Select = new[] { "attendees", "start", "end", "seriesMasterId" },
                        StartDateTime = context.TimeRange.Start.ToString("s"),
                        EndDateTime = context.TimeRange.End.ToString("s")
                    };
                }, stoppingToken)
                ;

            var pageIterator = PageIterator<Event, EventCollectionResponse>
                .CreatePageIterator(_graphClient, mailsResponse,
                async @event =>
                {
                    if (!string.IsNullOrEmpty(@event.SeriesMasterId))
                    {
                        var recurrenceSerie = await _graphClient
                        .Users[userEmail]
                        .Calendar
                        .Events[@event.SeriesMasterId]
                        .GetAsync(x =>
                        {
                            x.QueryParameters = new EventItemRequestBuilder.EventItemRequestBuilderGetQueryParameters()
                            {
                                Select = new[] { "recurrence" }
                            };
                        });
                        @event.Recurrence = recurrenceSerie.Recurrence;
                    }

                    var interactions = interactionFactory.CreateForUser(@event, userEmail);
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