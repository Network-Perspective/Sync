using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class RelationsCollection
    {
        public static readonly RelationsCollection Empty = new RelationsCollection(Array.Empty<Relation>());

        private readonly IEnumerable<Relation> _relations;

        public bool IsHashed { get; }


        public RelationsCollection(IEnumerable<Relation> relations) : this(relations, false)
        { }

        private RelationsCollection(IEnumerable<Relation> relations, bool isHashed)
        {
            _relations = relations;
            IsHashed = isHashed;
        }


        public RelationsCollection Hash(HashFunction hashFunction)
        {
            var hashedRelations = _relations.Select(x => x.Hash(hashFunction));
            return new RelationsCollection(hashedRelations, true);
        }

        public bool Contains(string relationName)
            => _relations.Any(x => x.Name == relationName);

        public string GetTargetEmployeeEmail(string relationName)
            => _relations.Single(x => x.Name == relationName).TargetEmployeeEmail;
    }
}