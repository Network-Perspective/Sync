using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.Core.Mappers
{
    internal static class EntitiesMapper
    {
        public static HashedEntity ToEntity(Employee employee, Employee employeeManager, string dataSourceIdName)
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
            }

            return new HashedEntity
            {
                Ids = IdsMapper.ToIds(employee, dataSourceIdName),
                Props = props,
                Groups = employee.Groups.Select(x => x.Id).ToList(),
                Relationships = ToRelationships(employee, employeeManager, dataSourceIdName),
                ChangeDate = DateTimeOffset.UtcNow
            };
        }

        private static ICollection<HashedEntityRelationship> ToRelationships(Employee employee, Employee employeeManager, string dataSourceIdName)
        {
            if (string.IsNullOrEmpty(employee.ManagerEmail))
                return null;

            return new List<HashedEntityRelationship>()
            {
                new HashedEntityRelationship
                {
                    RelationshipName = "Supervisor",
                    TargetIds = IdsMapper.ToIds(employeeManager, dataSourceIdName)
                }
            };
        }
    }
}