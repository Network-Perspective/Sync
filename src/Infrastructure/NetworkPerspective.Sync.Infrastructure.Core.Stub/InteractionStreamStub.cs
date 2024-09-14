using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;
using NetworkPerspective.Sync.Utils.Batching;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.Interactions;
using NetworkPerspective.Sync.Worker.Application.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Core.Stub
{
    internal class InteractionStreamStub : IInteractionsStream
    {
        private readonly SecureString _accessToken;
        private readonly FileDataWriter _fileWriter;
        private readonly Batcher<HashedInteraction> _batcher;
        private readonly string _dataSourceIdName;

        public InteractionStreamStub(SecureString accessToken, FileDataWriter fileWriter, NetworkPerspectiveCoreConfig npCoreConfig, string dataSourceIdName)
        {
            _accessToken = accessToken;
            _fileWriter = fileWriter;
            _dataSourceIdName = dataSourceIdName;
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
                .Select(x => InteractionMapper.DomainIntractionToDto(x, _dataSourceIdName))
                .ToList();

            await _batcher.AddRangeAsync(interactionsToPush);

            return interactionsToPush.Count();
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