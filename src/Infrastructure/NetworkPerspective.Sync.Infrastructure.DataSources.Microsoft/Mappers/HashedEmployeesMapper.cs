﻿using System.Collections.Generic;
using System.Linq;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

using DomainChannel = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Channel;
using Group = NetworkPerspective.Sync.Worker.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers
{
    internal static class HashedEmployeesMapper
    {
        public static EmployeeCollection ToEmployees(IEnumerable<User> users, IEnumerable<DomainChannel> channels, HashFunction.Delegate hashFunc, EmployeeFilter emailFilter)
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

            var usersChannels = channels.Where(x => x.UserIds.Contains(user.Mail));
            var usersChannelsGroups = usersChannels.Select(x => Group.Create(x.Id, $"{x.Team.Name} / {x.Name}", Group.ChannelCategory));
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