using System.Collections.Generic;
using System.Linq;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Extensions;

using DomainChannel = NetworkPerspective.Sync.Infrastructure.Microsoft.Models.Channel;
using Group = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers
{
    internal static class HashedEmployeesMapper
    {
        public static EmployeeCollection ToEmployees(IEnumerable<User> users, IEnumerable<DomainChannel> channels, HashFunction.Delegate hashFunc, EmailFilter emailFilter)
        {
            var employees = new List<Employee>();

            foreach (var user in users)
            {
                if (user.Mail is null) continue;

                var employeeGroups = GetEmployeeGroups(user, channels);
                var employeeProps = GetEmployeeProps(user);
                var employeeRelations = GetEmployeeRelations(user);

                var employeeId = EmployeeId.CreateWithAliases(user.Mail, user.Id, user.OtherMails, emailFilter);
                var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations);

                employees.Add(employee);
            }

            return new EmployeeCollection(employees, hashFunc);
        }

        private static IEnumerable<Group> GetEmployeeGroups(User user, IEnumerable<DomainChannel> channels)
        {
            var result = new List<Group>();

            var userDepartmentsGroups = user.GetDepartmentGroups();
            result.AddRange(userDepartmentsGroups);

            var useersChannels = channels.Where(x => x.UserIds.Contains(user.Mail));
            var usersChannelsGroups = useersChannels.Select(x => Group.Create(x.Id.ChannelId, x.Name, "Channel"));
            result.AddRange(usersChannelsGroups);

            return result;
        }

        private static IDictionary<string, object> GetEmployeeProps(User user)
        {
            return new Dictionary<string, object>();
        }

        private static RelationsCollection GetEmployeeRelations(User user)
        {
            var relations = new List<Relation>();

            if (user.Manager is User manager)
            {
                if (manager.Mail is not null)
                    relations.Add(Relation.Create(Relation.SupervisorRelationName, manager.Mail));
            }

            return new RelationsCollection(relations);
        }
    }
}