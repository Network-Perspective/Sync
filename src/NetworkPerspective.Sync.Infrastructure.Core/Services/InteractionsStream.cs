using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Batching;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Core.Services
{
    internal sealed class InteractionsStream : IInteractionsStream
    {
        private readonly NetworkPerspectiveCoreConfig _npCoreConfig;
        private readonly SecureString _accessToken;
        private readonly ISyncHashedClient _client;
        private readonly ILogger<IInteractionsStream> _logger;
        private readonly CancellationToken _stoppingToken;
        private readonly Batcher<HashedInteraction> _batcher;
        private bool _disposed = false;

        public InteractionsStream(SecureString accessToken, ISyncHashedClient client, NetworkPerspectiveCoreConfig npCoreConfig, ILogger<InteractionsStream> logger, CancellationToken stoppingToken)
        {
            _npCoreConfig = npCoreConfig;
            _accessToken = accessToken;
            _client = client;
            _logger = logger;
            _stoppingToken = stoppingToken;
            _batcher = new Batcher<HashedInteraction>(npCoreConfig.MaxInteractionsPerRequestCount);
            _batcher.OnBatchReady(PushAsync);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            await TryFlushAsync();
            _accessToken?.Dispose();

            _disposed = true;
        }

        public async Task<int> SendAsync(IEnumerable<Interaction> interactions)
        {
            var interactionsToPush = interactions
                .Select(x => InteractionMapper.DomainIntractionToDto(x, _npCoreConfig.DataSourceIdName))
                .ToList();

            await _batcher.AddRangeAsync(interactionsToPush, _stoppingToken);

            return interactions.Count();
        }

        public Task FlushAsync()
            => _batcher.FlushAsync();

        private async Task PushAsync(BatchReadyEventArgs<HashedInteraction> args)
        {
            _logger.LogInformation("Pushing interactions with count {count} hashed interactions to {url}...", args.BatchItems.Count, _npCoreConfig.BaseUrl);

            var command = new SyncHashedInteractionsCommand
            {
                ServiceToken = _accessToken.ToSystemString(),
                Interactions = args.BatchItems.ToArray(),
            };

            var response = await _client.SyncInteractionsAsync(command, _stoppingToken);

            _logger.LogInformation("Pushed interactions with count {count} hashed interactions to {url} successfully, correlationId: '{correlationId}'", args.BatchItems.Count, _npCoreConfig.BaseUrl, response);
        }

        private async Task TryFlushAsync()
        {
            try
            {
                await FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unsuccessful {method}", nameof(FlushAsync));
            }
        }
    }
}