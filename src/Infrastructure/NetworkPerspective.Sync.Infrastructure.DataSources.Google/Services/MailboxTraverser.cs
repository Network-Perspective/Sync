using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Exceptions;

using static Google.Apis.Gmail.v1.UsersResource.MessagesResource.GetRequest;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services
{

    internal class MailboxTraverser
    {
        public bool ReachedEndOfMailbox { get; private set; } = false;
        public int FetchedMessagesCount { get; private set; } = 0;

        private const int MaxResults = 500;

        private readonly string _email;
        private readonly int _maxMessagesCount;
        private readonly GmailService _service;
        private readonly IRetryPolicyProvider _retryPolicyProvider;
        private readonly ILogger<MailboxTraverser> _logger;

        private readonly List<string> _currentPageMessageIds = new List<string>();
        private string _nextPageToken = string.Empty;


        public MailboxTraverser(string email, int maxMessagesCount, GmailService service, IRetryPolicyProvider retryPolicyProvider, ILogger<MailboxTraverser> logger)
        {
            _email = email;
            _maxMessagesCount = maxMessagesCount;
            _service = service;
            _retryPolicyProvider = retryPolicyProvider;
            _logger = logger;
        }

        public async Task<Message> GetNextMessageAsync(CancellationToken stoppingToken)
        {
            if (FetchedMessagesCount >= _maxMessagesCount)
                throw new TooManyMailsPerUserException(_email);

            if (!_currentPageMessageIds.Any() && !ReachedEndOfMailbox)
                await LoadNextPageMessageIds(stoppingToken);

            if (!_currentPageMessageIds.Any())
                return null;

            var messageId = PullNextMessageId();

            FetchedMessagesCount++;

            return await GetMessageAsync(messageId, stoppingToken);
        }

        private async Task LoadNextPageMessageIds(CancellationToken stoppingToken)
        {
            _logger.LogTrace("Loading new messages page for user ***...");
            var messagesListRequest = _service.Users.Messages.List(_email);
            messagesListRequest.MaxResults = MaxResults;
            messagesListRequest.PageToken = _nextPageToken;

            var messagesListResponse = await _retryPolicyProvider
                .GetThrottlingRetryPolicy()
                .ExecuteAsync(messagesListRequest.ExecuteAsync, stoppingToken);

            _currentPageMessageIds.AddRange(messagesListResponse.Messages?.Select(x => x.Id) ?? Array.Empty<string>());
            _nextPageToken = messagesListResponse.NextPageToken;

            _logger.LogTrace("Loaded new message page for user ***...");

            if (string.IsNullOrEmpty(_nextPageToken))
            {
                _logger.LogTrace("End of mailbox reached for user ***");
                ReachedEndOfMailbox = true;
            }
        }

        private string PullNextMessageId()
        {
            var messageId = _currentPageMessageIds.First();
            _currentPageMessageIds.Remove(messageId);
            return messageId;
        }

        private async Task<Message> GetMessageAsync(string messageId, CancellationToken stoppingToken)
        {
            var singleMessageRequest = _service.Users.Messages.Get(_email, messageId);
            singleMessageRequest.Format = FormatEnum.Metadata;
            singleMessageRequest.MetadataHeaders = new Repeatable<string>(new[] { "from", "to", "cc", "bcc" });

            return await _retryPolicyProvider
                .GetThrottlingRetryPolicy()
                .ExecuteAsync(singleMessageRequest.ExecuteAsync, stoppingToken);
        }
    }
}