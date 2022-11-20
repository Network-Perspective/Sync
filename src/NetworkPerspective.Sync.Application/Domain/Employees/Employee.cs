using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NetworkPerspective.Sync.Application.Exceptions;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class Employee
    {
        public const string PropKeyTeam = "Team";
        public const string PropKeyDepartment = "Department";
        public const string PropKeyHierarchy = "Hierarchy";
        public const string PropKeyCreationTime = "CreationTime";
        public const string PropKeyName = "Name";

        public const string SupervisorRelationName = "Supervisor";

        public static readonly IEqualityComparer<Employee> EqualityComparer = new EmployeeEqualityComparer();

        public EmployeeId Id { get; init; }
        public bool IsExternal { get; init; }
        public bool IsBot { get; init; }
        public bool IsHashed { get; init; }
        public bool HasManager => Relations.Contains(SupervisorRelationName);
        public string ManagerEmail => HasManager ? Relations.GetTargetEmployeeEmail(SupervisorRelationName) : string.Empty;

        public IDictionary<string, object> Props { get; init; }
        public IReadOnlyCollection<Group> Groups { get; init; }
        public RelationsCollection Relations { get; init; }

        public Employee()
        {

        }

        private Employee(EmployeeId id, IEnumerable<Group> groups, bool isExternal, bool isBot, bool isHashed, IDictionary<string, object> props, RelationsCollection relations)
        {
            Id = id;
            IsExternal = isExternal;
            IsBot = isBot;
            IsHashed = isHashed;
            Relations = relations;
            Groups = groups.ToList();

            if (groups.Any())
            {
                props[PropKeyTeam] = GetTeam();
                props[PropKeyDepartment] = GetDepartments();
            }

            Props = props;
        }

        public static Employee CreateInternal(EmployeeId id, IEnumerable<Group> groups, IDictionary<string, object> props = null, RelationsCollection relations = null)
            => new Employee(id, groups, false, false, false, props ?? new Dictionary<string, object>(), relations ?? RelationsCollection.Empty);

        public static Employee CreateExternal(string email)
            => new Employee(EmployeeId.Create(email, string.Empty), Array.Empty<Group>(), true, false, false, ImmutableDictionary<string, object>.Empty, RelationsCollection.Empty);

        public static Employee CreateBot(string email)
            => new Employee(EmployeeId.Create(email, string.Empty), Array.Empty<Group>(), false, true, false, ImmutableDictionary<string, object>.Empty, RelationsCollection.Empty);

        public Employee Hash(HashFunction hashFunc)
        {
            if (IsHashed)
                throw new DoubleHashingException(nameof(Employee));

            var hashedId = Id.Hash(hashFunc);
            var hashedGroups = Groups.Select(x => x.Hash(hashFunc));
            var hashedRelations = Relations.Hash(hashFunc);

            return new Employee(hashedId, hashedGroups, IsExternal, IsBot, true, Props.ToDictionary(x => x.Key, y => y.Value), hashedRelations);
        }

        public void SetHierarchy(EmployeeHierarchy hierarchy)
            => Props[PropKeyHierarchy] = hierarchy;

        public EmployeeHierarchy GetHierarchy()
            => Props.ContainsKey(PropKeyHierarchy) ? (EmployeeHierarchy)Props[PropKeyHierarchy] : EmployeeHierarchy.Unknown;

        private string GetTeam()
            => GetTeamGroup()?.Id ?? string.Empty;

        private IEnumerable<string> GetDepartments()
            => GetDepartmentGroups().Select(x => x.Name);

        private Group GetTeamGroup()
            => Groups.SingleOrDefault(x => string.Equals(x.Category, Group.TeamCatergory, StringComparison.InvariantCultureIgnoreCase));

        private IEnumerable<Group> GetDepartmentGroups()
            => Groups.Where(x => string.Equals(x.Category, Group.DepartmentCatergory, StringComparison.InvariantCultureIgnoreCase));
    }
}