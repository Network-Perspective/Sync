using System.Collections.Generic;
using System.Linq;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;

using Group = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers
{
    public static class HashedEmployeesMapper
    {
        public static EmployeeCollection ToEmployees(IEnumerable<User> users, HashFunction.Delegate hashFunc)
        {
            var employees = new List<Employee>();

            foreach (var user in users)
            {
                var employeeGroups = GetEmployeeGroups(user);
                var employeeProps = GetEmployeeProps(user);
                var employeeRelations = GetEmployeeRelations(user);

                var employeeId = EmployeeId.CreateWithAliases(user.Mail, user.Id, user.OtherMails);
                var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations);

                employees.Add(employee);
            }

            return new EmployeeCollection(employees, hashFunc);
        }

        private static IEnumerable<Group> GetEmployeeGroups(User user)
        {
            return Enumerable.Empty<Group>();
        }

        private static IDictionary<string, object> GetEmployeeProps(User user)
        {
            return new Dictionary<string, object>();
        }

        private static RelationsCollection GetEmployeeRelations(User user)
        {
            return new RelationsCollection(Enumerable.Empty<Relation>());
        }
    }
}