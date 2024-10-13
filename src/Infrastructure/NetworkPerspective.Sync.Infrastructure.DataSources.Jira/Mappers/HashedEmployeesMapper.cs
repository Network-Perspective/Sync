using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Model;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Mappers;

internal static class HashedEmployeesMapper
{
    public static EmployeeCollection ToEmployees(IEnumerable<ProjectMember> members, HashFunction.Delegate hashFunc, EmployeeFilter emailFilter)
    {
        var employees = new List<Employee>();

        foreach (var member in members)
        {
            var employeeGroups = GetEmployeeGroups(member);
            var employeeProps = GetEmployeeProps(member);
            var employeeRelations = GetEmployeeRelations(member);

            var employeeId = EmployeeId.Create(member.Mail, member.Id);
            var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations);

            employees.Add(employee);
        }


        return new EmployeeCollection(employees, hashFunc);
    }

    private static IEnumerable<Group> GetEmployeeGroups(ProjectMember member)
    {
        return member.Projects.Select(x => Group.Create(x.Id, x.Name, Group.ProjectCategory));
    }

    private static IDictionary<string, object> GetEmployeeProps(ProjectMember member)
    {
        return new Dictionary<string, object>();
    }

    private static RelationsCollection GetEmployeeRelations(ProjectMember member)
    {
        var relations = new List<Relation>();

        return new RelationsCollection(relations);
    }
}