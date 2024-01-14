﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Extensions;

using Group = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers
{
    internal static class EmployeesMapper
    {
        public static EmployeeCollection ToEmployees(IEnumerable<User> users, HashFunction.Delegate hashFunc, EmployeeFilter emailFilter)
        {
            var employees = new List<Employee>();

            foreach (var user in users)
            {
                if (user.Mail is null) continue;

                var employeeGroups = GetEmployeeGroups(user);
                var employeeGroupsAccess = GetEmployeeGroupsAccess(user, hashFunc);
                var employeeProps = GetEmployeeProps(user);
                var employeeRelations = GetEmployeeRelations(user);

                var employeeId = EmployeeId.CreateWithAliases(user.Mail, user.Id, user.OtherMails, emailFilter);
                var employee = Employee.CreateInternal(employeeId, employeeGroups, employeeProps, employeeRelations, employeeGroupsAccess);

                employees.Add(employee);
            }

            return new EmployeeCollection(employees, null);
        }

        private static IEnumerable<Group> GetEmployeeGroups(User user)
        {
            return user.GetDepartmentGroups();
        }

        private static IEnumerable<string> GetEmployeeGroupsAccess(User user, HashFunction.Delegate hashFunc)
        {
            var result = new List<string>();

            var userDepartmentsGroups = user.GetDepartmentGroups();
            result.AddRange(userDepartmentsGroups.Select(x => hashFunc(x.Id)));

            return result;
        }

        private static IDictionary<string, object> GetEmployeeProps(User user)
        {
            var props = new Dictionary<string, object>();

            props.Add(Employee.PropKeyName, user.GetFullName());

            if (user.CreatedDateTime.HasValue)
            {
                var bucketAccCreationDate = new DateTime(user.CreatedDateTime.Value.Year, user.CreatedDateTime.Value.Month, 1);
                props.Add(Employee.PropKeyCreationTime, bucketAccCreationDate);
            }

            return props;
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