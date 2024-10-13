using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Model;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Mappers;

internal static class EmployeesMapper
{
    public static EmployeeCollection ToEmployees(IEnumerable<ProjectMember> projectMembers, HashFunction.Delegate hashFunc, EmployeeFilter emailFilter, bool syncGroupAccess)
    {
        var employees = new List<Employee>();

        foreach (var projectMember in projectMembers)
        {
            var employeeGroups = GetEmployeeGroups(projectMember);

            var employeeGroupsAccess = syncGroupAccess
                ? GetEmployeeGroupsAccess(projectMember, hashFunc)
                : null;

            var employeeProps = GetEmployeeProps(projectMember);
            var employeeRelations = GetEmployeeRelations(projectMember);
            var employeeId = EmployeeId.Create(projectMember.Mail, projectMember.Id);
            var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations, employeeGroupsAccess);

            employees.Add(employee);
        }

        return new EmployeeCollection(employees, null);
    }

    private static IEnumerable<Group> GetEmployeeGroups(ProjectMember projectMember)
    {
        var groups = projectMember.Projects.Select(x => Group.Create(x.Id, x.Name, Group.ProjectCategory));

        return groups;
    }

    private static IEnumerable<string> GetEmployeeGroupsAccess(ProjectMember projectMember, HashFunction.Delegate hashFunc)
        => GetEmployeeGroups(projectMember).Select(x => hashFunc(x.Id));

    private static IDictionary<string, object> GetEmployeeProps(ProjectMember projectMember)
    {
        var props = new Dictionary<string, object>();

        return props;
    }

    private static RelationsCollection GetEmployeeRelations(ProjectMember projectMember)
    {
        var relations = new List<Relation>();

        return new RelationsCollection(relations);
    }
}