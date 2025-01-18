using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Employees
{
    public class EmployeeId
    {
        public static readonly IEqualityComparer<EmployeeId> EqualityComparer = new EmployeeIdEqualityComparer();
        public static readonly EmployeeId Empty = new EmployeeId(string.Empty, string.Empty, string.Empty, Array.Empty<string>(), false);

        public string PrimaryId { get; }
        public string DataSourceId { get; }
        public string Username { get; }
        public IEnumerable<string> Aliases { get; }
        public bool IsHashed { get; }

        private EmployeeId(string primaryId, string dataSourceId, string username, IEnumerable<string> aliases, bool isHashed)
        {
            PrimaryId = primaryId;
            Aliases = aliases;
            IsHashed = isHashed;
            DataSourceId = dataSourceId;
            Username = username;
        }

        public static EmployeeId Create(string primaryId, string dataSourceId)
        {
            var userName = primaryId?.Split('@').FirstOrDefault();
            return new EmployeeId(primaryId, dataSourceId, userName, Array.Empty<string>(), false);
        }

        public static EmployeeId CreateWithAliases(string primaryId, string dataSourceId,
            IEnumerable<string> aliases, EmployeeFilter emailFilter)
        {
            var enumeratedAliases = aliases as string[] ?? aliases.ToArray();

            var matchingEmail = primaryId;
            if (emailFilter != null && !emailFilter.IsInternal(matchingEmail))
            {
                // find username matching whitelist 
                matchingEmail = enumeratedAliases?.FirstOrDefault(emailFilter.IsInternal);
            }
            var userName = matchingEmail?.Split('@').FirstOrDefault();

            return new EmployeeId(primaryId, dataSourceId, userName, enumeratedAliases, false);
        }


        public EmployeeId Hash(HashFunction.Delegate hashFunction)
        {
            var hashedPrimaryId = hashFunction(PrimaryId);
            var hashedDataSourceId = string.IsNullOrEmpty(DataSourceId) ? DataSourceId : hashFunction(DataSourceId);
            var hashedAliases = Aliases.Select(x => hashFunction(x));
            var hashedUsername = hashFunction(Username);

            return new EmployeeId(hashedPrimaryId, hashedDataSourceId, hashedUsername, hashedAliases, true);
        }

        public override string ToString()
            => $"{PrimaryId} ({string.Join(", ", Aliases)})";

        public override int GetHashCode()
            => PrimaryId.GetHashCode();
    }
}