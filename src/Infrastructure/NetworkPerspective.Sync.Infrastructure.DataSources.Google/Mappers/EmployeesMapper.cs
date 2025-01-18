using System;
using System.Collections.Generic;
using System.Linq;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Services;

using Group = NetworkPerspective.Sync.Worker.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Mappers;

internal interface IEmployeesMapper
{
    EmployeeCollection ToEmployees(IEnumerable<User> users, IEmployeePropsSource employeePropsSource);
}

internal class EmployeesMapper(ICompanyStructureService companyStructureService, ICustomAttributesService customAttributesService, ISyncContextAccessor syncContextAccessor, IHashingService hashingService) : IEmployeesMapper
{
    public EmployeeCollection ToEmployees(IEnumerable<User> users, IEmployeePropsSource employeePropsSource)
    {
        var connectorProperties = new GoogleConnectorProperties(syncContextAccessor.SyncContext.ConnectorProperties);

        var employees = new List<Employee>();

        var organizationGroups = companyStructureService
            .CreateGroups(users.Select(x => x.OrgUnitPath));

        foreach (var user in users)
        {
            var employeeGroups = GetEmployeeGroups(user, organizationGroups);
            var groupAccess = connectorProperties.SyncGroupAccess
                ? employeeGroups.Select(x => hashingService.Hash(x.Id))
                : null;
            var employeeProps = GetEmployeeProps(user, employeePropsSource);
            var employeeRelations = GetEmployeeRelations(user);

            var employeeAliases = user.Emails.Select(x => x.Address).ToHashSet();
            var employeeId = EmployeeId.CreateWithAliases(user.PrimaryEmail, user.Id, employeeAliases, syncContextAccessor.SyncContext.NetworkConfig.EmailFilter);
            var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations, groupAccess);

            employees.Add(employee);
        }

        return new EmployeeCollection(employees, null);
    }

    private static List<Group> GetEmployeeGroups(User user, ISet<Group> organizationGroups)
    {
        var userGroups = user
            .GetDepartmentGroups()
            .ToList();

        var userOrganizationGroupsIds = user.GetOrganizationGroupsIds();
        var userOrganizationGroups = organizationGroups.Where(x => userOrganizationGroupsIds.Any(y => y == x.Id));
        userGroups.AddRange(userOrganizationGroups);

        return userGroups;
    }

    private IDictionary<string, object> GetEmployeeProps(User user, IEmployeePropsSource employeePropsSource)
    {
        var props = customAttributesService.GetPropsForEmployee(user.GetCustomAttrs());
        props.Add(Employee.PropKeyName, user.GetFullName());

        var accCreationDate = user.GetAccountCreationDate();

        if (accCreationDate.HasValue)
        {
            var bucketAccCreationDate = new DateTime(accCreationDate.Value.Year, accCreationDate.Value.Month, 1);
            props.Add(Employee.PropKeyCreationTime, bucketAccCreationDate);
        }

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