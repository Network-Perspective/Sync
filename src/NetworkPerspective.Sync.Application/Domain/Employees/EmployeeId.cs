using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class EmployeeId
    {
        public static readonly IEqualityComparer<EmployeeId> EqualityComparer = new EmployeeIdEqualityComparer();
        public static readonly EmployeeId Empty = new EmployeeId(string.Empty, string.Empty, Array.Empty<string>(), false);

        public string PrimaryId { get; }
        public string DataSourceId { get; }
        public IEnumerable<string> Aliases { get; }
        public bool IsHashed { get; }

        private EmployeeId(string primaryId, string dataSourceId, IEnumerable<string> aliases, bool isHashed)
        {
            PrimaryId = primaryId;
            Aliases = aliases;
            IsHashed = isHashed;
            DataSourceId = dataSourceId;
        }

        public static EmployeeId Create(string primaryId, string dataSourceId)
            => new EmployeeId(primaryId, dataSourceId, Array.Empty<string>(), false);

        public static EmployeeId CreateWithAliases(string primaryId, string dataSourceId, IEnumerable<string> aliases)
            => new EmployeeId(primaryId, dataSourceId, aliases, false);

        public EmployeeId Hash(HashFunction hashFunction)
        {
            var hashedPrimaryId = hashFunction(PrimaryId);
            var hashedDataSourceId = string.IsNullOrEmpty(DataSourceId) ? DataSourceId : hashFunction(DataSourceId);
            var hashedAliases = Aliases.Select(x => hashFunction(x));

            return new EmployeeId(hashedPrimaryId, hashedDataSourceId, hashedAliases, true);
        }

        public override string ToString()
            => $"{PrimaryId} ({string.Join(", ", Aliases)})";

        public override int GetHashCode()
            => PrimaryId.GetHashCode();
    }
}