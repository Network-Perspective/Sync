using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Common.Tests
{
    public class NoopFilter : IInteractionsFilter
    {
        public ISet<Interaction> Filter(IEnumerable<Interaction> interactions)
            => interactions.ToHashSet();
    }
}
