using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Employees
{
    public class RelationsCollection : IEnumerable<Relation>
    {
        public static readonly RelationsCollection Empty = new RelationsCollection(Array.Empty<Relation>());

        private readonly IEnumerable<Relation> _relations;

        public bool IsHashed { get; init; }


        public RelationsCollection(IEnumerable<Relation> relations) : this(relations, false)
        { }

        private RelationsCollection(IEnumerable<Relation> relations, bool isHashed)
        {
            _relations = relations;
            IsHashed = isHashed;
        }

        public RelationsCollection Hash(HashFunction.Delegate hashFunction)
        {
            var hashedRelations = _relations.Select(x => x.Hash(hashFunction));
            return new RelationsCollection(hashedRelations, true);
        }

        public IEnumerable<Relation> GetAll()
            => _relations.ToList();

        public bool Contains(string relationName)
            => _relations.Any(x => x.Name == relationName);

        public string GetTargetEmployeeEmail(string relationName)
            => _relations.FirstOrDefault(x => x.Name == relationName)?.TargetEmployeeEmail;

        public IEnumerator<Relation> GetEnumerator()
        {
            return _relations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _relations.GetEnumerator();
        }
    }
}