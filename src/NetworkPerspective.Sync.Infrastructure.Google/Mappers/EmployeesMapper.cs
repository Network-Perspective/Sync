using System;
using System.Collections.Generic;
using System.Linq;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
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
        private readonly IEmployeePropsSource _employeePropsSource;
        private readonly EmailFilter _emailFilter;

        public EmployeesMapper(
            ICompanyStructureService companyStructureService,
            ICustomAttributesService customAttributesService,
            IEmployeePropsSource employeePropsSource,
            EmailFilter emailFilter)
        {
            _companyStructureService = companyStructureService;
            _customAttributesService = customAttributesService;
            _employeePropsSource = employeePropsSource;
            _emailFilter = emailFilter;
        }

        public EmployeeCollection ToEmployees(IEnumerable<User> users)
        {
            var employees = new List<Employee>();

            var organizationGroups = _companyStructureService
                .CreateGroups(users.Select(x => x.OrgUnitPath));

            foreach (var user in users)
            {
                var employeeGroups = GetEmployeeGroups(user, organizationGroups);
                var employeeProps = GetEmployeeProps(user);
                var employeeRelations = GetEmployeeRelations(user);

                var employeeAliases = user.Emails.Select(x => x.Address).ToHashSet();
                var employeeId = EmployeeId.CreateWithAliases(user.PrimaryEmail, user.Id, employeeAliases, _emailFilter);
                var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations);

                employees.Add(employee);
            }

            return new EmployeeCollection(employees, null);
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

            props = _employeePropsSource.EnrichProps(user.PrimaryEmail, props);

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