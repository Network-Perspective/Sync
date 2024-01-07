using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

using NetworkPerspective.Sync.Application.Exceptions;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class Employee
    {
        public const string PropKeyTeam = "Team";
        public const string PropKeyTeamCode = "TeamCode";
        public const string PropKeyDepartment = "Department";
        public const string PropKeyHierarchy = "Hierarchy";
        public const string PropKeyCreationTime = "CreationTime";
        public const string PropKeyEmploymentDate = "EmploymentDate";
        public const string PropKeyName = "Name";

        public static readonly IEqualityComparer<Employee> EqualityComparer = new EmployeeEqualityComparer();

        private readonly IList<Group> _groups;
        private readonly IList<string> _groupAccess;
        private readonly IDictionary<string, object> _props;

        public EmployeeId Id { get; }
        public bool IsExternal { get; }
        public bool IsBot { get; }
        public bool IsHashed { get; }
        public bool HasManager => Relations.Contains(Relation.SupervisorRelationName);
        public string ManagerEmail => HasManager ? Relations.GetTargetEmployeeEmail(Relation.SupervisorRelationName) : string.Empty;

        public IReadOnlyDictionary<string, object> Props => _props.ToImmutableDictionary();
        public IReadOnlyCollection<Group> Groups => new ReadOnlyCollection<Group>(_groups);
        public IReadOnlyCollection<string> GroupAccess => new ReadOnlyCollection<string>(_groupAccess);
        public RelationsCollection Relations { get; }

        private Employee(EmployeeId id, IEnumerable<Group> groups, bool isExternal, bool isBot, bool isHashed, IDictionary<string, object> props, RelationsCollection relations, IEnumerable<string> groupAccess)
        {
            Id = id;
            IsExternal = isExternal;
            IsBot = isBot;
            IsHashed = isHashed;
            _props = props;
            Relations = relations;
            _groups = groups.ToList();
            _groupAccess = groupAccess.ToList();

            if (_groups.Any())
            {
                var team = GetTeam();
                if (!string.IsNullOrEmpty(team)) _props[PropKeyTeam] = team;

                var departments = GetDepartments();
                if (departments.Any()) _props[PropKeyDepartment] = departments;
            }
        }

        public static Employee CreateInternal(EmployeeId id, IEnumerable<Group> groups, IDictionary<string, object> props = null, RelationsCollection relations = null, IEnumerable<string> groupAccess = null)
            => new Employee(id, groups, false, false, false, props ?? new Dictionary<string, object>(), relations ?? RelationsCollection.Empty, groupAccess ?? Enumerable.Empty<string>());

        public static Employee CreateExternal(string email)
            => new Employee(EmployeeId.Create(email, string.Empty), Array.Empty<Group>(), true, false, false, ImmutableDictionary<string, object>.Empty, RelationsCollection.Empty, Enumerable.Empty<string>());

        public static Employee CreateBot(string email)
            => new Employee(EmployeeId.Create(email, string.Empty), Array.Empty<Group>(), false, true, false, ImmutableDictionary<string, object>.Empty, RelationsCollection.Empty, Enumerable.Empty<string>());

        public Employee Hash(HashFunction.Delegate hashFunc)
        {
            if (IsHashed)
                throw new DoubleHashingException(nameof(Employee));

            var hashedId = Id.Hash(hashFunc);
            var hashedGroups = Groups.Select(x => x.Hash(hashFunc));
            var hashedRelations = Relations.Hash(hashFunc);

            var hashedProps = Props.ToDictionary(x => x.Key, y => y.Value);

            // set team code
            var team = GetTeamGroup();
            if (team != null)
            {
                hashedProps[PropKeyTeamCode] = hashFunc(team.Id);
            }

            return new Employee(hashedId, hashedGroups, IsExternal, IsBot, true, hashedProps, hashedRelations, GroupAccess);
        }

        public void SetHierarchy(EmployeeHierarchy hierarchy)
            => _props[PropKeyHierarchy] = hierarchy;

        public EmployeeHierarchy GetHierarchy()
            => _props.ContainsKey(PropKeyHierarchy) ? (EmployeeHierarchy)_props[PropKeyHierarchy] : EmployeeHierarchy.Unknown;

        private string GetTeam()
            => GetTeamGroup()?.Id ?? string.Empty;

        private IEnumerable<string> GetDepartments()
            => GetDepartmentGroups().Select(x => x.Name);

        private Group GetTeamGroup()
         => Groups.SingleOrDefault(x => string.Equals(x.Category, Group.TeamCatergory, StringComparison.InvariantCultureIgnoreCase));


        private IEnumerable<Group> GetDepartmentGroups() =>
            Groups.Where(x =>
                string.Equals(x.Category, Group.DepartmentCatergory, StringComparison.InvariantCultureIgnoreCase));
    }
}