using System.Collections.Generic;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Exceptions;

namespace NetworkPerspective.Sync.Application.Domain.Interactions
{
    public class InteractionVertex
    {
        public static readonly IEqualityComparer<InteractionVertex> EqualityComparer = new InteractionVertexEqualityComparer(new EmployeeIdEqualityComparer());

        public EmployeeId Id { get; init; }
        public bool IsExternal { get; init; }
        public bool IsBot { get; init; }
        public bool IsHashed { get; init; }

        public InteractionVertex()
        { }

        private InteractionVertex(EmployeeId id, bool isExternal, bool isBot, bool isHashed)
        {
            Id = id;
            IsExternal = isExternal;
            IsBot = isBot;
            IsHashed = isHashed;
        }

        public static InteractionVertex Create(EmployeeId id, bool isExternal, bool isBot)
            => new InteractionVertex(id, isExternal, isBot, false);

        public InteractionVertex Hash(HashFunction hashFunction)
        {
            if (IsHashed)
                throw new DoubleHashingException(nameof(InteractionVertex));

            return new InteractionVertex(Id.Hash(hashFunction), IsExternal, IsBot, true);
        }

    }
}
