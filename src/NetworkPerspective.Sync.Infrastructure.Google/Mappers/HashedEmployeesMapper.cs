using System.Collections.Generic;
using System.Linq;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

using Group = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.Google.Mappers
{
    internal class HashedEmployeesMapper : IEmployeesMapper
    {
        private readonly ICompanyStructureService _companyStructureService;
        private readonly ICustomAttributesService _customAttributesService;
        private readonly HashFunction.Delegate _hashFunc;

        public HashedEmployeesMapper(ICompanyStructureService companyStructureService, ICustomAttributesService customAttributesService, HashFunction.Delegate hashFunc)
        {
            _companyStructureService = companyStructureService;
            _customAttributesService = customAttributesService;
            _hashFunc = hashFunc;
        }

        public EmployeeCollection ToEmployees(IEnumerable<User> users)
        {
            var employees = new List<Employee>();

            var organizationGroups = _companyStructureService
                .CreateGroups(users.Select(x => x.OrgUnitPath));

            foreach (var user in users)
            {
                var customAttr = user.GetCustomAttrs();

                var employeeGroups = GetEmployeeGroups(user, organizationGroups);
                var employeeProps = GetEmployeeProps(user);
                var employeeRelations = GetEmployeeRelations(user);

                var employeeAliases = user.Emails.Select(x => x.Address).ToHashSet();
                var employeeId = EmployeeId.CreateWithAliases(user.PrimaryEmail, user.Id, employeeAliases);
                var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations);

                employees.Add(employee);
            }

            return new EmployeeCollection(employees, _hashFunc);
        }

        private IEnumerable<Group> GetEmployeeGroups(User user, ISet<Group> organizationGroups)
        {
            var userGroups = user
                .GetDepartmentGroups()
                .ToList();

            var userOrganizationGroupsIds = user.GetOrganizationGroupsIds();
            var userOrganizationGroups = organizationGroups.Where(x => userOrganizationGroupsIds.Any(y => y == x.Id));
            userGroups.AddRange(userOrganizationGroups);

            var customAttributesGroups = _customAttributesService.GetGroupsForHashedEmployee(user.GetCustomAttrs());
            userGroups.AddRange(customAttributesGroups);

            return userGroups;
        }

        private IDictionary<string, object> GetEmployeeProps(User user)
        {
            var props = _customAttributesService.GetPropsForHashedEmployee(user.GetCustomAttrs());

            return props;
        }

        private RelationsCollection GetEmployeeRelations(User user)
        {
            var customAttrs = user.GetCustomAttrs();

            var relations = _customAttributesService.GetRelations(customAttrs);

            var managerEmail = user.GetManagerEmail();

            if (!string.IsNullOrEmpty(managerEmail))
                relations.Add(Relation.Create(Relation.SupervisorRelationName, managerEmail));

            return new RelationsCollection(relations);
        }
    }
}