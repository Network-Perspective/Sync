using System.Collections.Generic;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Application.Services
{
    internal sealed class FilteredInteractionStreamDecorator : IInteractionsStream
    {
        private readonly IInteractionsStream _innerStream;
        private readonly IInteractionsFilter _filter;

        public FilteredInteractionStreamDecorator(IInteractionsStream innerStream, IInteractionsFilter filter)
        {
            _innerStream = innerStream;
            _filter = filter;
        }

        public ValueTask DisposeAsync()
            => _innerStream.DisposeAsync();

        public async Task SendAsync(IEnumerable<Interaction> interactions)
        {
            var filteredInteractions = _filter.Filter(interactions);

            await _innerStream.SendAsync(filteredInteractions);
        }
    }
}