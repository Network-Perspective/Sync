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
        public const string PropKeyDepartment = "Department";
        public const string PropKeyHierarchy = "Hierarchy";
        public const string PropKeyCreationTime = "CreationTime";
        public const string PropKeyName = "Name";

        public static readonly IEqualityComparer<Employee> EqualityComparer = new EmployeeEqualityComparer();

        private readonly IList<Group> _groups;
        private readonly IDictionary<string, object> _props;

        public string Email { get; }
        public string SourceInternalId { get; }
        public string ManagerEmail { get; }
        public bool IsExternal { get; }
        public bool IsBot { get; }
        public bool IsHashed { get; }
        public IReadOnlyDictionary<string, object> Props => _props.ToImmutableDictionary();
        public IReadOnlyCollection<Group> Groups => new ReadOnlyCollection<Group>(_groups);

        private Employee(string email, string sourceInternalId, string managerEmail, IEnumerable<Group> groups, bool isExternal, bool isBot, bool isHashed, IDictionary<string, object> props)
        {
            Email = email;
            SourceInternalId = sourceInternalId;
            ManagerEmail = managerEmail;
            IsExternal = isExternal;
            IsBot = isBot;
            IsHashed = isHashed;
            _props = props;
            _groups = groups.ToList();

            if (groups.Any())
            {
                _props[PropKeyTeam] = GetTeam();
                _props[PropKeyDepartment] = GetDepartments();
            }
        }

        public static Employee CreateInternal(string email, string sourceInternalId, string managerEmail, IEnumerable<Group> groups, IDictionary<string, object> props = null)
            => new Employee(email, sourceInternalId, managerEmail, groups, false, false, false, props ?? new Dictionary<string, object>());

        public static Employee CreateExternal(string email)
            => new Employee(email, string.Empty, string.Empty, Array.Empty<Group>(), true, false, false, ImmutableDictionary<string, object>.Empty);

        public static Employee CreateBot(string email)
            => new Employee(email, string.Empty, string.Empty, Array.Empty<Group>(), false, true, false, ImmutableDictionary<string, object>.Empty);

        public Employee Hash(Func<string, string> hashFunc)
        {
            if (IsHashed)
                throw new DoubleHashingException(nameof(Employee));

            var hashedEmail = hashFunc(Email);
            var hashedSourceInternalId = string.IsNullOrEmpty(SourceInternalId) ? SourceInternalId : hashFunc(SourceInternalId);
            var hashedManagerEmail = string.IsNullOrEmpty(ManagerEmail) ? ManagerEmail : hashFunc(ManagerEmail);
            var hashedGroups = Groups.Select(x => x.Hash(hashFunc));

            return new Employee(hashedEmail, hashedSourceInternalId, hashedManagerEmail, hashedGroups, IsExternal, IsBot, true, Props.ToDictionary(x => x.Key, y => y.Value));
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

        private IEnumerable<Group> GetDepartmentGroups()
            => Groups.Where(x => string.Equals(x.Category, Group.DepartmentCatergory, StringComparison.InvariantCultureIgnoreCase));
    }
}