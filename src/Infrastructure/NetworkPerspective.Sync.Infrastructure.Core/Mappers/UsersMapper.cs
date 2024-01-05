using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.Core.Mappers
{
    internal static class UsersMapper
    {
        public static UserEntity ToUser(Employee employee, string dataSourceIdName)
        {
            var props = employee.Props.Any()
                ? employee.Props.ToDictionary(x => x.Key, y => y.Value)
                : null;

            if (props is not null)
            {
                if (props.ContainsKey(Employee.PropKeyCreationTime))
                    props.Remove(Employee.PropKeyCreationTime);

                if (props.ContainsKey(Employee.PropKeyHierarchy))
                {
                    var hierarchy = (EmployeeHierarchy)props[Employee.PropKeyHierarchy];
                    props[Employee.PropKeyHierarchy] = MapHierarchy(hierarchy);
                }
            }

            var user = new UserEntity
            {
                Email = employee.Id.PrimaryId,
                Ids = IdsMapper.ToIds(employee, dataSourceIdName),
                Props = props,
                GroupAccess = employee.GroupAccess?.ToList()
            };

            return user;
        }

        private static string MapHierarchy(EmployeeHierarchy hierarchy)
        {
            return hierarchy switch
            {
                EmployeeHierarchy.Board => "Board",
                EmployeeHierarchy.IndividualContributor => "Individual contributor",
                EmployeeHierarchy.Manager => "Manager",
                EmployeeHierarchy.Director => "Director",
                EmployeeHierarchy.Unknown => "Unknown",
                _ => "Unknown",
            };
        }
    }
}