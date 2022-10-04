using System;
using System.Collections.Generic;
using System.Linq;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

using Group = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.Google.Mappers
{
    internal class EmployeesMapper : IEmployeesMapper
    {
        private readonly ICompanyStructureService _companyStructureService;
        private readonly ICustomAttributesService _customAttributesService;

        public EmployeesMapper(ICompanyStructureService companyStructureService, ICustomAttributesService customAttributesService)
        {
            _companyStructureService = companyStructureService;
            _customAttributesService = customAttributesService;
        }

        public EmployeeCollection ToEmployees(IEnumerable<User> users)
        {
            var employees = new EmployeeCollection(null);

            var organizationGroups = _companyStructureService
                .CreateGroups(users.Select(x => x.OrgUnitPath));

            foreach (var user in users)
            {
                var employeeGroups = GetEmployeeGroups(user, organizationGroups);
                var employeeProps = GetEmployeeProps(user);
                var employeeRelations = GetEmployeeRelations(user);

                var employeeAliases = user.Emails.Select(x => x.Address).ToHashSet();
                var employeeId = EmployeeId.CreateWithAliases(user.PrimaryEmail, user.Id, employeeAliases);
                var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations);

                employees.Add(employee);
            }

            return employees;
        }

        private static IEnumerable<Group> GetEmployeeGroups(User user, ISet<Group> organizationGroups)
        {
            var userGroups = user
                .GetDepartmentGroups()
                .ToList();

            var userOrganizationGroupsIds = user.GetOrganizationGroupsIds();
            var userOrganizationGroups = organizationGroups.Where(x => userOrganizationGroupsIds.Any(y => y == x.Id));
            userGroups.AddRange(userOrganizationGroups);

            return userGroups;
        }

        private IDictionary<string, object> GetEmployeeProps(User user)
        {
            var props = _customAttributesService.GetPropsForEmployee(user.GetCustomAttrs());
            props.Add(Employee.PropKeyName, user.GetFullName());

            var accCreationDate = user.GetAccountCreationDate();

            if (accCreationDate.HasValue)
            {
                var bucketAccCreationDate = new DateTime(accCreationDate.Value.Year, accCreationDate.Value.Month, 1);
                props.Add(Employee.PropKeyCreationTime, bucketAccCreationDate);
            }

            return props;
        }

        private RelationsCollection GetEmployeeRelations(User user)
        {
            var relations = new List<Relation>();

            var managerEmail = user.GetManagerEmail();

            if (!string.IsNullOrEmpty(managerEmail))
                relations.Add(Relation.Create(Employee.SupervisorRelationName, managerEmail));

            return new RelationsCollection(relations);
        }
    }
}