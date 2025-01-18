using System.Collections.Generic;
using System.Linq;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Services;

using Group = NetworkPerspective.Sync.Worker.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Mappers;

internal interface IHashedEmployeesMapper
{
    EmployeeCollection ToEmployees(IEnumerable<User> users, IEmployeePropsSource employeePropsSource);
}

internal class HashedEmployeesMapper(ICompanyStructureService companyStructureService, ICustomAttributesService customAttributesService, ISyncContextAccessor syncContextAccessor, IHashingService hashingService) : IHashedEmployeesMapper
{

    public EmployeeCollection ToEmployees(IEnumerable<User> users, IEmployeePropsSource employeePropsSource)
    {
        var employees = new List<Employee>();

        var organizationGroups = companyStructureService
            .CreateGroups(users.Select(x => x.OrgUnitPath));

        foreach (var user in users)
        {
            var customAttr = user.GetCustomAttrs();

            var employeeGroups = GetEmployeeGroups(user, organizationGroups);
            var employeeProps = GetEmployeeProps(user, employeePropsSource);
            var employeeRelations = GetEmployeeRelations(user);

            var employeeAliases = user.Emails.Select(x => x.Address).ToHashSet();
            var employeeId = EmployeeId.CreateWithAliases(user.PrimaryEmail, user.Id, employeeAliases, syncContextAccessor.SyncContext.NetworkConfig.EmailFilter);
            var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations);

            employees.Add(employee);
        }

        return new EmployeeCollection(employees, hashingService.Hash);
    }

    private IEnumerable<Group> GetEmployeeGroups(User user, ISet<Group> organizationGroups)
    {
        var userGroups = user
            .GetDepartmentGroups()
            .ToList();

        var userOrganizationGroupsIds = user.GetOrganizationGroupsIds();
        var userOrganizationGroups = organizationGroups.Where(x => userOrganizationGroupsIds.Any(y => y == x.Id));
        userGroups.AddRange(userOrganizationGroups);

        var customAttributesGroups = customAttributesService.GetGroupsForHashedEmployee(user.GetCustomAttrs());
        userGroups.AddRange(customAttributesGroups);

        return userGroups;
    }

    private IDictionary<string, object> GetEmployeeProps(User user, IEmployeePropsSource employeePropsSource)
    {
        var props = customAttributesService.GetPropsForHashedEmployee(user.GetCustomAttrs());

        props = employeePropsSource.EnrichProps(user.PrimaryEmail, props);

        return props;
    }

    private RelationsCollection GetEmployeeRelations(User user)
    {
        var customAttrs = user.GetCustomAttrs();

        var relations = customAttributesService.GetRelations(customAttrs);

        var managerEmail = user.GetManagerEmail();

        if (!string.IsNullOrEmpty(managerEmail))
            relations.Add(Relation.Create(Relation.SupervisorRelationName, managerEmail));

        return new RelationsCollection(relations);
    }
}