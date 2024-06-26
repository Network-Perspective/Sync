﻿using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Batching;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Core.Stub
{
    internal class InteractionStreamStub : IInteractionsStream
    {
        private readonly SecureString _accessToken;
        private readonly FileDataWriter _fileWriter;
        private readonly Batcher<HashedInteraction> _batcher;
        private readonly NetworkPerspectiveCoreConfig _npCoreConfig;

        public InteractionStreamStub(SecureString accessToken, FileDataWriter fileWriter, NetworkPerspectiveCoreConfig npCoreConfig)
        {
            _accessToken = accessToken;
            _fileWriter = fileWriter;
            _npCoreConfig = npCoreConfig;

            _batcher = new Batcher<HashedInteraction>(npCoreConfig.MaxInteractionsPerRequestCount);
            _batcher.OnBatchReady(PushAsync);
        }

        public async ValueTask DisposeAsync()
        {
            await _batcher.FlushAsync();
        }

        public async Task<int> SendAsync(IEnumerable<Interaction> interactions)
        {
            var interactionsToPush = interactions
                .Select(x => InteractionMapper.DomainIntractionToDto(x, _npCoreConfig.DataSourceIdName))
                .ToList();

            await _batcher.AddRangeAsync(interactionsToPush);

            return interactionsToPush.Count;
        }

        private async Task PushAsync(BatchReadyEventArgs<HashedInteraction> args)
        {
            if (args.BatchItems.Any())
            {
                var fileName = $"{_accessToken.ToSystemString().GetStableHashCode()}_{args.BatchItems.First().When:yyyyMMdd_HHmmss}_interactions.json";
                await _fileWriter.WriteAsync(args.BatchItems, fileName);
            }
        }
    }
}