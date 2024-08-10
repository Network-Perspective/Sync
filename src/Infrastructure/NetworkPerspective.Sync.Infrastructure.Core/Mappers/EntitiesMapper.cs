using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.Core.Mappers
{
    internal static class EntitiesMapper
    {
        public static HashedEntity ToEntity(Employee employee, EmployeeCollection employees, DateTime changeDate, string dataSourceIdName)
        {
            var props = employee.Props.Any()
                ? employee.Props.ToDictionary(x => x.Key, y => y.Value)
                : null;

            if (props is not null)
            {
                if (props.ContainsKey(Employee.PropKeyName))
                    props.Remove(Employee.PropKeyName);

                if (props.ContainsKey(Employee.PropKeyTeam))
                    props.Remove(Employee.PropKeyTeam);

                if (props.ContainsKey(Employee.PropKeyDepartment))
                    props.Remove(Employee.PropKeyDepartment);

                if (props.ContainsKey(Employee.PropKeyHierarchy))
                    props.Remove(Employee.PropKeyHierarchy);
            }

            return new HashedEntity
            {
                Ids = IdsMapper.ToIds(employee, dataSourceIdName),
                Props = props,
                Groups = employee.Groups.Select(x => x.Id).ToList(),
                Relationships = ToRelationships(employee, employees, dataSourceIdName),
                ChangeDate = changeDate
            };
        }

        private static ICollection<HashedEntityRelationship> ToRelationships(Employee employee, EmployeeCollection employees, string dataSourceIdName)
        {
            var result = new List<HashedEntityRelationship>();

            foreach (var relation in employee.Relations.GetAll())
            {
                var targetEmployee = employees.Find(relation.TargetEmployeeEmail);

                var hashedEntityRelationship = new HashedEntityRelationship
                {
                    RelationshipName = relation.Name,
                    TargetIds = IdsMapper.ToIds(targetEmployee, dataSourceIdName)
                };

                result.Add(hashedEntityRelationship);
            }

            return result.Any() ? result : null;

        }
    }
}