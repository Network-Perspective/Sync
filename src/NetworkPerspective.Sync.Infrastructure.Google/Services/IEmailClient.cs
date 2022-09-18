using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Aggregation;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal interface IEmailClient
    {
        Task<ISet<Interaction>> GetInteractionsAsync(Guid networkId, IEnumerable<Employee> userEmails, DateTime startDate, GoogleCredential credentials, InteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal sealed class EmailClient : IEmailClient
    {
        private static readonly int MinutesInDay = 24 * 60;

        private readonly GoogleConfig _config;
        private readonly ILogger<EmailClient> _logger;
        private readonly IThrottlingRetryHandler _retryHandler = new ThrottlingRetryHandler();
        private readonly IStatusLogger _statusLogger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IClock _clock;

        public EmailClient(IStatusLogger statusService, IOptions<GoogleConfig> config, ILoggerFactory loggerFactory, IClock clock)
        {
            _config = config.Value;
            _logger = loggerFactory.CreateLogger<EmailClient>();
            _statusLogger = statusService;
            _loggerFactory = loggerFactory;
            _clock = clock;
        }

        public async Task<ISet<Interaction>> GetInteractionsAsync(Guid networkId, IEnumerable<Employee> userEmails, DateTime startDate, GoogleCredential credentials, InteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Evaluating interactions based on mailbox since {timestamp} for {count} users...", startDate, userEmails.Count());

            var result = new HashSet<Interaction>(new InteractionEqualityComparer());
            var maxMessagesCountPerUser = CalculateMaxMessagesCount(startDate);

            foreach (var userEmail in userEmails)
            {
                try
                {
                    var interactions = await GetSingleUserInteractionsAsync(userEmail.Email, maxMessagesCountPerUser, startDate, credentials, interactionFactory, stoppingToken);
                    result.UnionWith(interactions);
                }
                catch (TooManyMailsPerUserException tmmpuex)
                {
                    await _statusLogger.LogWarningAsync(networkId, $"Skipping mailbox '{tmmpuex.Email}' too many messages", stoppingToken);
                    _logger.LogWarning("Skipping mailbox '{email}' too many messages", tmmpuex.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to evaluate interactions based on mailbox for given user. Please see inner exception\n");
                }
            }

            _logger.LogInformation("Evaluation of interactions based on mailbox since '{timestamp}' completed", startDate);

            return result;
        }

        private async Task<ISet<Interaction>> GetSingleUserInteractionsAsync(string userEmail, int maxMessagesCount, DateTime startDate, GoogleCredential credentials, InteractionFactory interactionFactory, CancellationToken stoppingToken)
        {
            _logger.LogDebug("Evaluating interactions based on mailbox for user ***...");

            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            using var gmailService = InitializeGmailService(userEmail, credentials);
            var actionsAggregator = new ActionsAggregator(userEmail);
            var mailboxTraverser = new MailboxTraverser(userEmail, maxMessagesCount, gmailService, _retryHandler, _loggerFactory.CreateLogger<MailboxTraverser>());
            var message = await mailboxTraverser.GetNextMessageAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested && message != null && message?.GetDateTime(_clock) > startDate)
            {
                actionsAggregator.Add(message.GetDateTime(_clock));
                var interactions = interactionFactory.CreateFromEmail(message);
                result.UnionWith(interactions);
                message = await mailboxTraverser.GetNextMessageAsync(stoppingToken);
            }

            _logger.LogDebug("Evaluation interactions based on mailbox for user '{user}' completed. Found {count} interactions out of {mailsCount} email/s", "***", result.Count, mailboxTraverser.FetchedMessagesCount);
            _logger.LogTrace("Evaluation interactions based on mailbox for user '{user}' completed. Found {count} interactions out of {mailsCount} email/s", userEmail, result.Count, mailboxTraverser.FetchedMessagesCount);

            _logger.LogTrace(new DefaultActionsAggregatorPrinter().Print(actionsAggregator));

            return result;
        }

        private int CalculateMaxMessagesCount(DateTime startDate)
        {
            var now = _clock.UtcNow();
            var totalMinutes = (now - startDate).TotalMinutes;

            var maxMessagesCount = (int)(_config.MaxMessagesPerUserDaily * totalMinutes / MinutesInDay);

            var result = maxMessagesCount > 0 ? maxMessagesCount : 1;

            _logger.LogDebug("For synchronization period {start} - {now} max allowed messages is {count}", startDate, now, result);

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

    }
}